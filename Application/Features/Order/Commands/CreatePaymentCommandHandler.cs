using Application.Common;
using Application.DTOs.Response;
using Application.DTOs.Services;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace Application.Features.Order.Commands
{
    public record CreatePaymentCommand(
        List<CartItemRequest> Items,
        string? CouponCode,
        string IpAddress,
        int TypePayment,
        string Address,
        string PhoneNumber,
        string FullName,
        string IdempotencyKey,
        int CustomerId
    ) : IRequest<Result<CreatePaymentResponse>>;


    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<CreatePaymentResponse>>
    {
        private readonly IVnPayService _vnPayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDatabase _redisConnection;
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IAppReadDbContext _db;

        public CreatePaymentCommandHandler(
            IVnPayService vnPayService,
            IUnitOfWork unitOfWork,
            IConnectionMultiplexer connectionMultiplexer,
            IMediator mediator,
            IPublishEndpoint publishEndpoint,
            IAppReadDbContext db)
        {
            _vnPayService = vnPayService;
            _unitOfWork = unitOfWork;
            _redisConnection = connectionMultiplexer.GetDatabase();
            _mediator = mediator;
            _publishEndpoint = publishEndpoint;
            _db = db;
        }

        public async Task<Result<CreatePaymentResponse>> Handle(CreatePaymentCommand request, CancellationToken ct)
        {
            string idempotencyKey = $"Idempotency:Payment:{request.IdempotencyKey}";
            
            if (request.TypePayment == 1)
            {
                var cachedResponse = await _redisConnection.StringGetAsync(idempotencyKey);
                if (cachedResponse.HasValue)
                {
                    if (cachedResponse == "PROCESSING")
                    {
                        return Result<CreatePaymentResponse>.Failure("Giao dịch đang được xử lý. Vui lòng không gửi lại.", 409);
                    }
                    var cached = JsonSerializer.Deserialize<CreatePaymentResponse>(cachedResponse!);
                    return Result<CreatePaymentResponse>.Success(cached!, 200);
                }

                bool lockAcquired = await _redisConnection.StringSetAsync(idempotencyKey, "PROCESSING", TimeSpan.FromMinutes(15), When.NotExists);
                if (!lockAcquired)
                {
                    return Result<CreatePaymentResponse>.Failure("Giao dịch đang được xử lý.", 409);
                }
            }

            var itemUserWantBuy = new Dictionary<int, int>();
            foreach (var item in request.Items)
            {
                if (itemUserWantBuy.ContainsKey(item.ColorId))
                {
                    itemUserWantBuy[item.ColorId] += item.Quantity;
                }
                else
                {
                    itemUserWantBuy[item.ColorId] = item.Quantity;
                }
            }

            int customerId = request.CustomerId;
            var successItems = new Dictionary<int, int>();

            try
            {
                foreach (var item in itemUserWantBuy)
                {
                    string cacheKeyStock = $"Color:Stock:{item.Key}";

                    var exists = await _redisConnection.KeyExistsAsync(cacheKeyStock);
                    if (!exists)
                    {
                        var dbStockMap = await _unitOfWork.InventoryRepository.GetStockByColorIdsAsync(new List<int> { item.Key }, ct);
                        int dbStock = dbStockMap.ContainsKey(item.Key) ? dbStockMap[item.Key] : 0;
                        await _redisConnection.StringSetAsync(cacheKeyStock, dbStock, TimeSpan.FromDays(1));
                    }

                    long newStock = await _redisConnection.StringDecrementAsync(cacheKeyStock, item.Value);

                    if (newStock < 0)
                    {
                        await _redisConnection.StringIncrementAsync(cacheKeyStock, item.Value);

                        foreach (var success in successItems)
                        {
                            await _redisConnection.StringIncrementAsync($"Color:Stock:{success.Key}", success.Value);
                        }

                        return Result<CreatePaymentResponse>.Failure("Sản phẩm đã hết hàng do có người khác vừa mua.", 400);
                    }

                    successItems.Add(item.Key, item.Value);
                }

                var calculateResult = await _mediator.Send(new CalculateOrderCommand(request.Items, request.CouponCode, customerId), ct);
                if (!calculateResult.IsSuccess)
                {
                    foreach (var success in successItems)
                    {
                        await _redisConnection.StringIncrementAsync($"Color:Stock:{success.Key}", success.Value);
                    }
                    return Result<CreatePaymentResponse>.Failure(calculateResult.ErrorCode ?? "Lỗi tính toán đơn hàng.", 400);
                }

                var priceInfo = calculateResult.Data!;

                List<int> colorIds = itemUserWantBuy.Keys.ToList();
                Dictionary<int, decimal> colorPrices = await _unitOfWork.ProductRepository.GetPricesByColorIdsAsync(colorIds, ct);

                var reservedVehicles = await _unitOfWork.InventoryRepository.ReserveVehiclesAsync(itemUserWantBuy, ct);

                var orderItems = new List<OrderItem>();
                foreach (var vehicle in reservedVehicles)
                {
                    if (!colorPrices.TryGetValue(vehicle.ColorId, out decimal unitPrice))
                    {
                        foreach (var success in successItems)
                        {
                            await _redisConnection.StringIncrementAsync($"Color:Stock:{success.Key}", success.Value);
                        }
                        await _unitOfWork.InventoryRepository.ReleaseVehiclesAsync(reservedVehicles.Select(v => v.VehicleId).ToList(), ct);
                        return Result<CreatePaymentResponse>.Failure($"Sản phẩm màu ID {vehicle.ColorId} không có thông tin giá hợp lệ.", 400);
                    }

                    orderItems.Add(new OrderItem
                    {
                        VehicleId = vehicle.VehicleId,
                        UnitPriceAtPurchase = unitPrice
                    });
                }

                string orderStatus = request.TypePayment == 0
                    ? OrderStatus.Pending.ToString()
                    : OrderStatus.Processing_Payment.ToString();

                string paymentStatus = request.TypePayment == 0
                    ? PaymentStatus.Unpaid.ToString()
                    : PaymentStatus.Pending.ToString();

                string provider = request.TypePayment == 0
                    ? PaymentProvider.COD.ToString()
                    : PaymentProvider.VnPay.ToString();

                var newOrder = new Domain.Entities.Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = priceInfo.FinalAmount,
                    DiscountAmount = priceInfo.DiscountAmount > 0 ? priceInfo.DiscountAmount : null,
                    CouponId = priceInfo.CouponId,
                    Status = orderStatus,
                    OrderItems = orderItems,
                };

                var shippingDetail = new OrderShippingDetail
                {
                    RecipientName = request.FullName,
                    RecipientPhone = request.PhoneNumber,
                    StreetAddress = request.Address,
                };
                newOrder.OrderShippingDetail = shippingDetail;

                var newPayment = new Payment
                {
                    Amount = priceInfo.FinalAmount,
                    Provider = provider,
                    PaymentStatus = paymentStatus,
                    IdempotencyKey = request.IdempotencyKey,
                };
                newOrder.Payment = newPayment;

                await _unitOfWork.OrderRepository.AddAsync(newOrder);
                await _unitOfWork.SaveChangesAsync(ct);

                if (priceInfo.CouponId.HasValue)
                {
                    var couponUsage = new CouponUsage
                    {
                        CouponId = priceInfo.CouponId.Value,
                        CustomerId = customerId,
                        OrderId = newOrder.OrderId,
                        UsedAt = DateTime.UtcNow
                    };
                    var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.CouponId == priceInfo.CouponId.Value, ct);
                    if (coupon != null)
                    {
                        coupon.UsedCount = (coupon.UsedCount ?? 0) + 1;
                    }
                    await _unitOfWork.SaveChangesAsync(ct);
                }
                if (request.TypePayment == 0) // COD
                {
                    var response = new CreatePaymentResponse
                    {
                        OrderId = newOrder.OrderId,
                        Message = "Đặt hàng thành công (COD)."
                    };
                    await _unitOfWork.CartRepository.DeleteCartItemsAsync(customerId, colorIds);
                    return Result<CreatePaymentResponse>.Success(response, 201);
                }
                else // VnPay
                {
                    var paymentUrl = _vnPayService.CreatePaymentUrl(newOrder.OrderId, priceInfo.FinalAmount, request.IpAddress);

                    await _publishEndpoint.Publish(new OrderTimeoutEvent(newOrder.OrderId), context =>
                    {
                        context.Delay = TimeSpan.FromMinutes(15);
                    }, ct);

                    var response = new CreatePaymentResponse
                    {
                        OrderId = newOrder.OrderId,
                        PaymentUrl = paymentUrl,
                        Message = "Vui lòng thanh toán trong 15 phút."
                    };
                    await _redisConnection.StringSetAsync(idempotencyKey,
                        JsonSerializer.Serialize(response), TimeSpan.FromHours(24));
                    return Result<CreatePaymentResponse>.Success(response, 201);
                }
            }
            catch (Exception)
            {
                foreach (var success in successItems)
                {
                    await _redisConnection.StringIncrementAsync($"Color:Stock:{success.Key}", success.Value);
                }
                if (request.TypePayment == 1)
                {
                    await _redisConnection.KeyDeleteAsync(idempotencyKey);
                }
                return Result<CreatePaymentResponse>.Failure("Đã xảy ra lỗi khi xử lý thanh toán.", 500);
            }
        }
    }
}
