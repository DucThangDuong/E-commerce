using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Application.Features.Order.Commands
{
    public record CancelOrderCommand(int OrderId, string Reason, string UserId) : IRequest<Result<string>>;
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<string>>
    {
        private readonly IAppReadDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDatabase _redisConnection;

        public CancelOrderCommandHandler(IAppReadDbContext db, IUnitOfWork unitOfWork, IConnectionMultiplexer connectionMultiplexer)
        {
            _db = db;
            _unitOfWork = unitOfWork;
            _redisConnection = connectionMultiplexer.GetDatabase();
        }

        public async Task<Result<string>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (!int.TryParse(request.UserId, out int customerId))
                    return Result<string>.Failure("Unauthorized", 401);

                var order = await _db.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Vehicle)
                            .ThenInclude(v => v.Color)
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.CustomerId == customerId, cancellationToken);

                if (order == null)
                    return Result<string>.Failure("Đơn hàng không tồn tại hoặc không thuộc quyền sở hữu của bạn.", 404);

                if (order.Status.Equals(Domain.Enums.OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase) || 
                    order.Status.Equals("Canceled", StringComparison.OrdinalIgnoreCase))
                    return Result<string>.Failure("Đơn hàng đã được hủy trước đó.", 400);

                if (!order.Status.Equals(Domain.Enums.OrderStatus.Pending.ToString(), StringComparison.OrdinalIgnoreCase))
                    return Result<string>.Failure("Chỉ có thể hủy đơn hàng đang ở trạng thái chờ xử lý (Pending).", 400);

                // Tìm ReasonId từ DB
                var cancellationReasons = await _db.CancellationReasons.ToListAsync(cancellationToken);
                var matchedReason = cancellationReasons.FirstOrDefault(x => x.Content.Equals(request.Reason, StringComparison.OrdinalIgnoreCase));
                
                int reasonId;
                string? customText = null;

                if (matchedReason != null)
                {
                    reasonId = matchedReason.ReasonId;
                }
                else
                {
                    var defaultReason = cancellationReasons.FirstOrDefault(x => x.Content.Contains("Lý do khác") || x.Code == "OTHER");
                    reasonId = defaultReason?.ReasonId ?? 1; 
                    customText = request.Reason;
                }

                order.Status = Domain.Enums.OrderStatus.Cancelled.ToString();
                order.UpdatedAt = DateTime.UtcNow;

                order.OrderCancellation = new OrderCancellation
                {
                    ReasonId = reasonId,
                    CustomReasonText = customText,
                    CanceledAt = DateTime.UtcNow,
                    CanceledByUserId = customerId
                };

                var vehicleIdsToRelease = order.OrderItems.Select(oi => oi.VehicleId).ToList();
                await _unitOfWork.InventoryRepository.ReleaseVehiclesAsync(vehicleIdsToRelease, cancellationToken);

                var groupedByColor = order.OrderItems.GroupBy(oi => oi.Vehicle.ColorId);
                foreach (var g in groupedByColor)
                {
                    string cacheKeyStock = $"Color:Stock:{g.Key}";
                    var exists = await _redisConnection.KeyExistsAsync(cacheKeyStock);
                    if (exists)
                    {
                        await _redisConnection.StringIncrementAsync(cacheKeyStock, g.Count());
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<string>.Success("Hủy đơn hàng thành công.", 200);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Có lỗi xảy ra: {ex.Message}", 500);
            }
        }
    }
}
