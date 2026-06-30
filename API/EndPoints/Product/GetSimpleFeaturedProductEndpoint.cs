using API.Extensions;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace API.EndPoints.Product
{
    public class GetSimpleFeaturedProductEndpoint : EndpointWithoutRequest
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetSimpleFeaturedProductEndpoint(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public override void Configure()
        {
            Get("/product/featured");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                string cacheKey = "simple_featured_products";
                var featuredProducts = await _cache.GetOrSetAsync(cacheKey, async () => 
                {
                    return await _db.FeaturedProducts
                        .AsNoTracking()
                        .Where(f => f.Product != null && (!f.Product.IsDeleted.HasValue || !f.Product.IsDeleted.Value))
                        .OrderBy(f => f.DisplayOrder)
                        .Select(f => new ResSimpleFeaturedProductDto
                        {
                            ProductId = f.ProductId,
                            DisplayOrder = f.DisplayOrder,
                            FirstColorImageUrl = f.Product.ProductColors
                                .Select(pc => pc.ProductImages.Select(pi => pi.ImageUrl).FirstOrDefault())
                                .FirstOrDefault()
                        })
                        .ToListAsync(ct);
                }, TimeSpan.FromHours(24));

                await this.SendApiResponseAsync(Application.Common.Result<List<ResSimpleFeaturedProductDto>>.Success(featuredProducts ?? new List<ResSimpleFeaturedProductDto>()), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Application.Common.Result<List<ResSimpleFeaturedProductDto>>.Failure($"Failed to retrieve simple featured products: {ex.Message}", 500), ct);
            }
        }
    }
}
