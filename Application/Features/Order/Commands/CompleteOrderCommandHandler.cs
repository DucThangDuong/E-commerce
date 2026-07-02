using Application.Common;
using Application.Interfaces;
using Domain.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Order.Commands
{
    public record CompleteOrderCommand(int OrderId) : IRequest<Result<string>>;

    public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppReadDbContext _db;

        public CompleteOrderCommandHandler(IUnitOfWork unitOfWork, IAppReadDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public async Task<Result<string>> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(request.OrderId);

            if (order == null)
            {
                return Result<string>.Failure("Không tìm thấy đơn hàng.", 404);
            }

            if (order.Status != OrderStatus.Shipping.ToString() && order.Status != OrderStatus.Confirmed.ToString())
            {
                return Result<string>.Failure("Chỉ có thể xác nhận nhận hàng đối với đơn hàng đang giao hoặc đã xác nhận.", 400);
            }

            order.Status = OrderStatus.Completed.ToString();
            order.UpdatedAt = DateTime.UtcNow;
            
            if (order.Payment != null && order.Payment.PaymentStatus != PaymentStatus.Paid.ToString())
            {
                order.Payment.PaymentStatus = PaymentStatus.Paid.ToString();
            }

            var orderItems = await _db.OrderItems
                .Include(oi => oi.Vehicle)
                .Where(oi => oi.OrderId == request.OrderId)
                .ToListAsync(cancellationToken);

            foreach (var item in orderItems)
            {
                if (item.Vehicle != null)
                {
                    item.Vehicle.Status = VehicleStatus.Sold.ToString();
                    var warranty = new WarrantyBook
                    {
                        VehicleId = item.VehicleId,
                        CustomerId = order.CustomerId,
                        ActivatedAt = DateTime.UtcNow,
                        ValidUntil = DateTime.UtcNow.AddYears(3) 
                    };
                    _db.WarrantyBooks.Add(warranty);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Đã xác nhận giao hàng thành công, tự động kích hoạt bảo hành điện tử cho xe!");
        }
    }
}
