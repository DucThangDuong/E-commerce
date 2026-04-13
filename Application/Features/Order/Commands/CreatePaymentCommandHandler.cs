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
                    var reservationValue = await _redisConnection.StringGetAsync($"Order:Reservation:{request.OrderId}");
                    var reservedItems = reservationValue.HasValue ? JsonSerializer.Deserialize<Dictionary<int, int>>(reservationValue) : null;
                    await _unitOfWork.InventoryRepository.UpdateDecreaseStockAsync(reservedItems);
                    await _unitOfWork.SaveChangesAsync();
                    await _redisConnection.KeyDeleteAsync($"Order:Reservation:{request.OrderId}");
                    return Result<string>.Success("Payment created successfully with COD method.");
                }
                else
                {
                    var paymentUrl = _vnPayService.CreatePaymentUrl(request.OrderId, request.Amount, request.IpAddress);
                    await _redisConnection.KeyDeleteAsync($"Order:Reservation:{request.OrderId}");
                    
                    return Result<string>.Success(paymentUrl);
                }
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(ex.Message, 500);
            }
        }
    }
}
