using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries
{
    public record GetSimpleFeaturedProductQuery() : IRequest<Result<List<ResSimpleFeaturedProductDto>>>;

    public class GetSimpleFeaturedProductHandler : IRequestHandler<GetSimpleFeaturedProductQuery, Result<List<ResSimpleFeaturedProductDto>>>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetSimpleFeaturedProductHandler(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Result<List<ResSimpleFeaturedProductDto>>> Handle(GetSimpleFeaturedProductQuery query, CancellationToken ct)
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

                return Result<List<ResSimpleFeaturedProductDto>>.Success(featuredProducts ?? new List<ResSimpleFeaturedProductDto>());
            }
            catch (Exception ex)
            {
                return Result<List<ResSimpleFeaturedProductDto>>.Failure($"Failed to retrieve simple featured products: {ex.Message}", 500);
            }
        }
    }
}
