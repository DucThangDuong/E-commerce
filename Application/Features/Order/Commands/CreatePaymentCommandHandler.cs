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
    public record CreatePaymentCommand(int OrderId, decimal Amount, string IpAddress,int TypePayment,string Address,string PhoneNumber) : IRequest<Result<string>>;

    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<string>>
    {
        private readonly IVnPayService _vnPayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDatabase _redisConnection;
        public CreatePaymentCommandHandler(IVnPayService vnPayService, IUnitOfWork unitOfWork, IConnectionMultiplexer connectionMultiplexer)
        {
            _vnPayService = vnPayService;
            _unitOfWork = unitOfWork;
            _redisConnection=connectionMultiplexer.GetDatabase();

        }

        public async Task<Result<string>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var reservationValue = await _redisConnection.StringGetAsync($"Order:Reservation:{request.OrderId}");
                if(!reservationValue.HasValue)
                {
                    return Result<string>.Failure("No reservation found for this order.", 404);
                }
                var reservedItems = JsonSerializer.Deserialize<Dictionary<int, int>>(reservationValue!);
                if (request.TypePayment == 0)
                {
                    Payment newPayment = new Payment
                    {
                        OrderId = request.OrderId,
                        Amount = request.Amount,
                        Provider = "COD",
                        PaymentStatus = "Pending",
                        Address = request.Address,
                        PhoneNumber = request.PhoneNumber
                    };
                    await _unitOfWork.PaymentRepository.AddAsync(newPayment);
                    await _unitOfWork.InventoryRepository.UpdateDecreaseStockAsync(reservedItems);
                    await _unitOfWork.SaveChangesAsync();
                    await _redisConnection.KeyDeleteAsync($"Order:Reservation:{request.OrderId}");
                    return Result<string>.Success("Payment created successfully with COD method.");
                }
                else
                {
                    var paymentUrl = _vnPayService.CreatePaymentUrl(request.OrderId, request.Amount, request.IpAddress);
                    await _redisConnection.KeyDeleteAsync($"Order:Reservation:{request.OrderId}");
                    
                    return Result<string>.Success(paymentUrl,201);
                }
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(ex.Message, 500);
            }
        }
    }
}
