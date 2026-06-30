using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Coupons.Commands
{
    public record CheckCouponCommand(
        string CouponCode,
        List<CartItemRequest> Items,
        int CustomerId
    ) : IRequest<Result<ResCheckCoupon>>;

    public class CheckCouponHandler : IRequestHandler<CheckCouponCommand, Result<ResCheckCoupon>>
    {
        private readonly IAppReadDbContext _db;

        private readonly IProductRepository _productRepository;
        public CheckCouponHandler(IAppReadDbContext db, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _db = db;
        }

        public async Task<Result<ResCheckCoupon>> Handle(CheckCouponCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.CouponCode))
            {
                return Result<ResCheckCoupon>.Failure("Mã giảm giá không được để trống.", 400);
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

            List<int> colorIds = itemUserWantBuy.Keys.ToList();
            Dictionary<int, decimal> colorPrices = await _productRepository.GetPricesByColorIdsAsync(colorIds, ct);
            
            decimal subTotal = 0;
            foreach (var item in itemUserWantBuy)
            {
                if (!colorPrices.TryGetValue(item.Key, out decimal unitPrice))
                {
                    return Result<ResCheckCoupon>.Failure($"Sản phẩm màu ID {item.Key} không có thông tin giá hợp lệ.", 400);
                }
                subTotal += unitPrice * item.Value;
            }

            var coupon = await _db.Coupons
                .Where(c => c.Code == request.CouponCode && c.IsActive == true)
                .FirstOrDefaultAsync(ct);

            if (coupon == null)
            {
                return Result<ResCheckCoupon>.Failure("Mã giảm giá không tồn tại hoặc đã bị vô hiệu hóa.", 400);
            }
            if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
            {
                return Result<ResCheckCoupon>.Failure("Mã giảm giá đã hết hạn sử dụng.", 400);
            }
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
            {
                return Result<ResCheckCoupon>.Failure("Mã giảm giá đã hết lượt sử dụng.", 400);
            }
            if (coupon.UsageLimitPerUser.HasValue)
            {
                int userUsageCount = await _db.CouponUsages
                    .CountAsync(cu => cu.CouponId == coupon.CouponId && cu.CustomerId == request.CustomerId, ct);

                if (userUsageCount >= coupon.UsageLimitPerUser.Value)
                {
                    return Result<ResCheckCoupon>.Failure("Bạn đã sử dụng hết số lần cho phép của mã giảm giá này.", 400);
                }
            }
            if (coupon.MinOrderValue.HasValue && subTotal < coupon.MinOrderValue.Value)
            {
                return Result<ResCheckCoupon>.Failure(
                    $"Đơn hàng phải có giá trị tối thiểu {coupon.MinOrderValue.Value:N0}đ để áp dụng mã này.", 400);
            }

            decimal discountAmount = 0;
            if (string.Equals(coupon.DiscountType, Domain.Enums.DiscountType.Percentage.ToString(), StringComparison.OrdinalIgnoreCase) || 
                string.Equals(coupon.DiscountType, "percentage", StringComparison.OrdinalIgnoreCase))
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

            decimal finalAmount = subTotal - discountAmount;

            return Result<ResCheckCoupon>.Success(new ResCheckCoupon
            {
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                CouponCode = coupon.Code,
                CouponName = coupon.Name,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue
            }, 200);
        }
    }
}
