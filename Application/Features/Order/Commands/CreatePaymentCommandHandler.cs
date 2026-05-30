using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using MediatR;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace Application.Features.Order.Commands
{
    public record CreatePaymentCommand(string ReservationId, decimal Amount, string IpAddress, int TypePayment, string Address,
        string PhoneNumber, string FullName, string idempotencyKey) : IRequest<Result<string>>;

    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<string>>
    {
        private readonly IVnPayService _vnPayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDatabase _redisConnection;
        public CreatePaymentCommandHandler(IVnPayService vnPayService, IUnitOfWork unitOfWork, IConnectionMultiplexer connectionMultiplexer)
        {
            _vnPayService = vnPayService;
            _unitOfWork = unitOfWork;
            _redisConnection = connectionMultiplexer.GetDatabase();
        }

        public async Task<Result<string>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            string idempotencyKey = $"Idempotency:Payment:{request.idempotencyKey}";
            var cachedResponse = await _redisConnection.StringGetAsync(idempotencyKey);
            if (cachedResponse.HasValue)
            {
                if (cachedResponse == "PROCESSING")
                {
                    return Result<string>.Failure("Giao dịch đang được xử lý. Vui lòng không gửi lại.", 409);
                }
                return Result<string>.Success(cachedResponse!, 200);
            }
            bool lockAcquired = await _redisConnection.StringSetAsync(idempotencyKey, "PROCESSING", TimeSpan.FromMinutes(15), When.NotExists);
            if (!lockAcquired)
            {
                return Result<string>.Failure("Giao dịch đang được xử lý.", 409);
            }
            string reservationLockKey = $"Order:ReservationLock:{request.ReservationId}";
            bool resLockAcquired = await _redisConnection.StringSetAsync(reservationLockKey, "PROCESSING", TimeSpan.FromMinutes(2), When.NotExists);
            if (!resLockAcquired)
            {
                return Result<string>.Failure("Đơn hàng đang được xử lý thanh toán.", 409);
            }

            try
            {
                string reservationKey = $"Order:Reservation:{request.ReservationId}";
                var reservationValue = await _redisConnection.StringGetAsync(reservationKey);
                if (!reservationValue.HasValue)
                {
                    return Result<string>.Failure("No reservation found for this order.", 404);
                }

                var reservationData = JsonSerializer.Deserialize<JsonElement>(reservationValue!);
                int customerId = reservationData.GetProperty("CustomerId").GetInt32();
                var itemsElement = reservationData.GetProperty("Items");
                var reservedItems = JsonSerializer.Deserialize<Dictionary<int, int>>(itemsElement.GetRawText());

                if (reservedItems == null || !reservedItems.Any())
                {
                    return Result<string>.Failure("Reservation data is invalid.", 400);
                }

                List<int> colorIds = reservedItems.Keys.ToList();
                Dictionary<int, decimal> colorPrices = await _unitOfWork.ProductRepository.GetPricesByColorIdsAsync(colorIds, cancellationToken);

                var orderItems = new List<OrderItem>();
                decimal totalAmount = 0;

                foreach (var item in reservedItems)
                {
                    int colorId = item.Key;
                    int quantity = item.Value;

                    if (!colorPrices.TryGetValue(colorId, out decimal unitPrice))
                    {
                        return Result<string>.Failure($"Màu sản phẩm ID {colorId} không có thông tin giá hợp lệ.");
                    }

                    orderItems.Add(new OrderItem
                    {
                        ColorId = colorId,
                        Quantity = quantity,
                        UnitPriceAtPurchase = unitPrice
                    });

                    totalAmount += unitPrice * quantity;
                }

                var newOrder = new Domain.Entities.Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = Domain.Enums.OrderStatus.Pending.ToString(),
                    OrderItems = orderItems,
                };

                var shippingDetail = new OrderShippingDetail
                {
                    RecipientName = request.FullName,
                    RecipientPhone = request.PhoneNumber,
                    StreetAddress = request.Address,
                };
                newOrder.OrderShippingDetail = shippingDetail;

                string provider = request.TypePayment == 0 ? Domain.Enums.PaymentProvider.COD.ToString() : Domain.Enums.PaymentProvider.VnPay.ToString();
                var newPayment = new Payment
                {
                    Amount = totalAmount,
                    Provider = provider,
                    PaymentStatus = Domain.Enums.PaymentStatus.Pending.ToString(),
                    IdempotencyKey= request.idempotencyKey,
                };
                newOrder.Payment = newPayment;

                await _unitOfWork.OrderRepository.AddAsync(newOrder);

                await _unitOfWork.InventoryRepository.UpdateDecreaseStockAsync(reservedItems);

                await _unitOfWork.SaveChangesAsync();

                await _redisConnection.KeyDeleteAsync(reservationKey);

                if (request.TypePayment == 0)
                {
                    await _unitOfWork.CartRepository.DeleteCartItemsAsync(customerId, colorIds);
                    return Result<string>.Success("Payment created successfully with COD method.", 201);
                }
                else
                {
                    var paymentUrl =  _vnPayService.CreatePaymentUrl(newOrder.OrderId, totalAmount, request.IpAddress);
                    await _redisConnection.StringSetAsync(idempotencyKey,
                        JsonSerializer.Serialize(paymentUrl), TimeSpan.FromHours(24));
                    return Result<string>.Success(paymentUrl, 201);
                }
            }
            catch (Exception ex)
            {
                await _redisConnection.KeyDeleteAsync(idempotencyKey);
                return Result<string>.Failure("An internal error occurred while processing the payment.", 500);
            }
            finally
            {
                await _redisConnection.KeyDeleteAsync(reservationLockKey);
            }
        }
    }
}
