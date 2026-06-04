using Microsoft.EntityFrameworkCore;
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
        private readonly MassTransit.IPublishEndpoint _publishEndpoint;
        private readonly IAppReadDbContext _db;

        public UpdateHasPaymentHandler(IUnitOfWork unitOfWork, INotificationService notificationService, MassTransit.IPublishEndpoint publishEndpoint, IAppReadDbContext db) { 
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _publishEndpoint = publishEndpoint;
            _db = db;
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
                    order.Status = OrderStatus.Failed.ToString();
                    if (order.Payment != null)
                    {
                        order.Payment.PaymentStatus = PaymentStatus.Payment_Mismatch.ToString();
                        order.Payment.ProviderTransactionId = request.TransactionNo;
                    }
                    
                    var customerEmail = await _db.Customers
                        .Where(c => c.CustomerId == order.CustomerId)
                        .Select(c => c.Email)
                        .FirstOrDefaultAsync(ct);

                    if (!string.IsNullOrEmpty(customerEmail))
                    {
                        string subject = "Cảnh báo: Thanh toán đơn hàng sai số tiền";
                        string body = $"Kính gửi quý khách,<br/><br/>Đơn hàng <b>#{order.OrderId}</b> của quý khách đã bị ghi nhận sai số tiền thanh toán từ cổng thanh toán. " +
                                      $"<br/>Số tiền thực tế của đơn hàng: {order.TotalAmount:N0}đ." +
                                      $"<br/>Số tiền hệ thống nhận báo cáo: {request.Amount:N0}đ." +
                                      $"<br/><br/>Đơn hàng tạm thời bị hủy bỏ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ đối soát." +
                                      $"<br/><br/>Trân trọng,";
                        await _publishEndpoint.Publish(new Application.DTOs.Services.SendMail(customerEmail, subject, body), ct);
                    }

                    await _unitOfWork.SaveChangesAsync(ct);

                    return Result<ResIpnDTO>.Failure("Invalid amount", 400, new ResIpnDTO { RspCode = "04", Message = "Invalid amount" });
                }

                // 3. Kiểm tra xem đơn hàng đã được cập nhật trước đó chưa (Chống gọi IPN nhiều lần)
                if (order.Status == OrderStatus.Confirmed.ToString() || 
                    order.Status == OrderStatus.Cancelled.ToString() || 
                    order.Status == OrderStatus.Failed.ToString() ||
                    order.Status == OrderStatus.Pending.ToString())
                {
                    return Result<ResIpnDTO>.Failure("Order already processed", 400, new ResIpnDTO { RspCode = "02", Message = "Order already confirmed" });
                }

                if (request.ResponseCode == "00")
                {
                    order.Status = OrderStatus.Confirmed.ToString(); 
                    if (order.Payment != null)
                    {
                        order.Payment.PaymentStatus = PaymentStatus.Paid.ToString();
                        order.Payment.ProviderTransactionId = request.TransactionNo;
                    }

                    await _notificationService.SendMessageToOrderId(
                        order.OrderId.ToString(), 
                        $"Thanh toán thành công đơn hàng #{order.OrderId}"
                    );
                }
                else
                {
                    if (order.Payment != null)
                    {
                        order.Payment.PaymentStatus = PaymentStatus.Fail.ToString();
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
