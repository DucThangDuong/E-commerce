using API.Extensions;
using Application.Common;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace API.EndPoints.Coupons
{
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

    public class GetActiveCouponsEndpoint : EndpointWithoutRequest
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetActiveCouponsEndpoint(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public override void Configure()
        {
            Get("/coupons/active");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var cacheKey = "ActiveCoupons";

            var activeCoupons = await _cache.GetOrSetAsync(cacheKey, async () => 
            {
                var now = DateTime.UtcNow;
                return await _db.Coupons
                    .AsNoTracking()
                    .Where(c => c.IsActive == true && c.StartDate <= now && c.EndDate >= now)
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
            }, TimeSpan.FromMinutes(30));

            await this.SendApiResponseAsync(Result<List<ActiveCouponResponse>>.Success(activeCoupons ?? new List<ActiveCouponResponse>()), ct);
        }
    }
}
