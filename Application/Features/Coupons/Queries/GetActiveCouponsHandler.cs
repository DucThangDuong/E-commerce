using Application.Common;
using Application.Interfaces;
using Application.IServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Coupons.Queries
{
    public record GetActiveCouponsQuery() : IRequest<Result<List<ActiveCouponResponse>>>;

    public record ActiveCouponResponse
    {
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string DiscountType { get; init; } = null!;
        public decimal DiscountValue { get; init; }
        public decimal? MinOrderValue { get; init; }
        public int? RemainingUsages { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }

    public class GetActiveCouponsHandler : IRequestHandler<GetActiveCouponsQuery, Result<List<ActiveCouponResponse>>>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetActiveCouponsHandler(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Result<List<ActiveCouponResponse>>> Handle(GetActiveCouponsQuery request, CancellationToken ct)
        {
            var cacheKey = "ActiveCoupons";

            var activeCoupons = await _cache.GetOrSetAsync(cacheKey, async () => 
            {
                var now = DateTime.UtcNow;
                return await _db.Coupons
                    .AsNoTracking()
                    .Where(c => c.IsActive == true && c.StartDate <= now && c.EndDate >= now)
                    // Also check if usage limit is not exceeded
                    .Where(c => c.UsageLimit == null || (c.UsedCount ?? 0) < c.UsageLimit)
                    .Select(c => new ActiveCouponResponse
                    {
                        Code = c.Code,
                        Name = c.Name,
                        DiscountType = c.DiscountType,
                        DiscountValue = c.DiscountValue,
                        MinOrderValue = c.MinOrderValue,
                        RemainingUsages = c.UsageLimit.HasValue ? (c.UsageLimit.Value - (c.UsedCount ?? 0)) : null,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate
                    })
                    .ToListAsync(ct);
            }, TimeSpan.FromMinutes(30)); // Caching for 30 minutes

            return Result<List<ActiveCouponResponse>>.Success(activeCoupons ?? new List<ActiveCouponResponse>());
        }
    }
}
