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

                // 3. Kiểm tra xem đơn hàng đã được cập nhật trước đó chưa (Chống gọi IPN nhiều lần)
                if (order.Status == "Confirmed" || order.Status == "Cancelled" || order.Status == "Failed")
                {
                    return Result<ResIpnDTO>.Failure("Order already processed", 400, new ResIpnDTO { RspCode = "02", Message = "Order already confirmed" });
                }

                // 4. Cập nhật trạng thái dựa vào vnp_ResponseCode
                if (request.ResponseCode == "00")
                {
                    // Khách thanh toán thành công
                    order.Status = "Confirmed"; // Hoặc trạng thái chuẩn bị giao hàng

                    // Lưu vết giao dịch
                    if (order.Payment != null)
                    {
                        order.Payment.PaymentStatus = "Paid";
                        order.Payment.ProviderTransactionId = request.TransactionNo;
                    }

                    // Bắn SignalR báo khách hàng thanh toán thành công
                    await _notificationService.SendMessageToOrderId(
                        order.OrderId.ToString(), 
                        $"Thanh toán thành công đơn hàng #{order.OrderId}"
                    );
                }
                else
                {
                    // Khách hủy thanh toán hoặc thanh toán lỗi
                    if (order.Payment != null)
                    {
                        order.Payment.PaymentStatus = "Fail";
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
