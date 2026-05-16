using Application.Common;
using Application.DTOs.Services;
using Application.Interfaces;
using Application.IServices;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Features.Order.Commands
{
    public record ProcessIpnCommand(int OrderId, decimal Amount, string ResponseCode, string TransactionNo) : IRequest<Result<ResIpnDTO>>;
    
    public class UpdateHasPaymentHandler : IRequestHandler<ProcessIpnCommand, Result<ResIpnDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public UpdateHasPaymentHandler(IUnitOfWork unitOfWork, INotificationService notificationService) { 
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Result<ResIpnDTO>> Handle(ProcessIpnCommand request, CancellationToken ct)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    return Result<ResIpnDTO>.Failure("Order not found", 404, new ResIpnDTO { RspCode = "01", Message = "Order not found" });
                }

                if (order.TotalAmount != request.Amount)
                {
                    return Result<ResIpnDTO>.Failure("Invalid amount", 400, new ResIpnDTO { RspCode = "04", Message = "Invalid amount" });
                }

                if (order.PaymentStatus == (int)Payment_status.Success || order.Status == (int)Order_status.Cancelled)
                {
                    return Result<ResIpnDTO>.Failure("Order already processed", 400, new ResIpnDTO { RspCode = "02", Message = "Order already confirmed" });
                }

                if (request.ResponseCode == "00")
                {
                    order.PaymentStatus = (int)Payment_status.Success;
                    order.Status = (int)Order_status.Completed; 
                    var payment = order.Payments.FirstOrDefault(p => p.Provider == "VnPay");
                    if (payment != null)
                    {
                        payment.PaymentStatus = "Paid";
                        payment.ProviderTransactionId = request.TransactionNo;
                    }
                    await _notificationService.SendMessageToOrderId(order.OrderId.ToString(), $"Thanh toán thành công đơn hàng #{order.OrderId}");
                }
                else
                {
                    var payment = order.Payments.FirstOrDefault(p => p.Provider == "VnPay");
                    if (payment != null)
                    {
                        payment.PaymentStatus = "Fail";
                    }
                }

                await _unitOfWork.SaveChangesAsync(ct);

                return Result<ResIpnDTO>.Success(new ResIpnDTO { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception)
            {
                return Result<ResIpnDTO>.Failure("An error occurred while processing the IPN", 500, new ResIpnDTO { RspCode = "99", Message = "Unknown error" });
            }
        }
    }
}
