using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Order.Commands
{
    public record CalculateOrderCommand(
        List<CartItemRequest> Items,
        string? CouponCode,
        int CustomerId
    ) : IRequest<Result<CalculateOrderResponse>>;

    public class CalculateOrderHandler : IRequestHandler<CalculateOrderCommand, Result<CalculateOrderResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppReadDbContext _db;

        public CalculateOrderHandler(IUnitOfWork unitOfWork, IAppReadDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public async Task<Result<CalculateOrderResponse>> Handle(CalculateOrderCommand request, CancellationToken ct)
        {
            // 1. Gom nhóm Items
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

            List<int> colorIds = itemUserWantBuy.Keys.ToList();

            Dictionary<int, decimal> colorPrices = await _unitOfWork.ProductRepository.GetPricesByColorIdsAsync(colorIds, ct);
            decimal subTotal = 0;

            foreach (var item in itemUserWantBuy)
            {
                if (!colorPrices.TryGetValue(item.Key, out decimal unitPrice))
                {
                    return Result<CalculateOrderResponse>.Failure($"Sản phẩm màu ID {item.Key} không có thông tin giá hợp lệ.", 400);
                }
                subTotal += unitPrice * item.Value;
            }
            decimal shippingFee = 0;

            decimal discountAmount = 0;
            int? couponId = null;
            string? couponCode = null;

            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var coupon = await _db.Coupons
                    .Where(c => c.Code == request.CouponCode && c.IsActive == true)
                    .FirstOrDefaultAsync(ct);

                if (coupon == null)
                {
                    return Result<CalculateOrderResponse>.Failure("Mã giảm giá không tồn tại hoặc đã bị vô hiệu hóa.", 400);
                }
                if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
                {
                    return Result<CalculateOrderResponse>.Failure("Mã giảm giá đã hết hạn sử dụng.", 400);
                }
                if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
                {
                    return Result<CalculateOrderResponse>.Failure("Mã giảm giá đã hết lượt sử dụng.", 400);
                }
                if (coupon.UsageLimitPerUser.HasValue)
                {
                    int userUsageCount = await _db.CouponUsages
                        .CountAsync(cu => cu.CouponId == coupon.CouponId && cu.CustomerId == request.CustomerId, ct);

                    if (userUsageCount >= coupon.UsageLimitPerUser.Value)
                    {
                        return Result<CalculateOrderResponse>.Failure("Bạn đã sử dụng hết số lần cho phép của mã giảm giá này.", 400);
                    }
                }
                if (coupon.MinOrderValue.HasValue && subTotal < coupon.MinOrderValue.Value)
                {
                    return Result<CalculateOrderResponse>.Failure(
                        $"Đơn hàng phải có giá trị tối thiểu {coupon.MinOrderValue.Value:N0}đ để áp dụng mã này.", 400);
                }

                if (coupon.DiscountType == Domain.Enums.DiscountType.Percentage.ToString())
                {
                    discountAmount = subTotal * coupon.DiscountValue / 100;
                    if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                    {
                        discountAmount = coupon.MaxDiscountAmount.Value;
                    }
                }
                else 
                {
                    discountAmount = coupon.DiscountValue;
                }

                if (discountAmount > subTotal)
                {
                    discountAmount = subTotal;
                }

                couponId = coupon.CouponId;
                couponCode = coupon.Code;
            }

            decimal finalAmount = subTotal + shippingFee - discountAmount;

            return Result<CalculateOrderResponse>.Success(new CalculateOrderResponse
            {
                SubTotal = subTotal,
                ShippingFee = shippingFee,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                CouponId = couponId,
                CouponCode = couponCode
            }, 200);
        }
    }
}
