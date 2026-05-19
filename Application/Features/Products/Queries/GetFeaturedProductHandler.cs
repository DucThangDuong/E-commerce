using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries
{
    public record GetFeaturedProductQuery() : IRequest<Result<List<ResFeaturedProductDto>>>;

    public class GetFeaturedProductHandler : IRequestHandler<GetFeaturedProductQuery, Result<List<ResFeaturedProductDto>>>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetFeaturedProductHandler(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Result<List<ResFeaturedProductDto>>> Handle(GetFeaturedProductQuery query, CancellationToken ct)
        {
            try
            {
                string cacheKey = "featured_products";
                var cachedProducts = await _cache.GetAsync<List<ResFeaturedProductDto>>(cacheKey);
                if (cachedProducts != null)
                {
                    return Result<List<ResFeaturedProductDto>>.Success(cachedProducts);
                }

                var featuredProducts = await _db.FeaturedProducts
                    .AsNoTracking()
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new ResFeaturedProductDto
                    {
                        FeaturedId = f.FeaturedId,
                        ProductId = f.ProductId,
                        DisplayOrder = f.DisplayOrder,
                        StartDate = f.StartDate,
                        EndDate = f.EndDate,
                        Product = new ResProductDto
                        {
                            BasePrice = f.Product.BasePrice,
                            CategoryId = f.Product.CategoryId,
                            BrandId = f.Product.BrandId,
                            Description = f.Product.Description,
                            Name = f.Product.Name,
                            ProductId = f.Product.ProductId,
                            ImageUrls = f.Product.ProductImages.Where(pi => pi.ColorId == null).Select(pi => pi.ImageUrl).ToList(),
                            Colors = f.Product.ProductColors.Select(pc => new ResProductColorDto
                            {
                                ColorId = pc.ColorId,
                                ColorName = pc.ColorName,
                                PriceAdjustment = pc.PriceAdjustment,
                                StockQuantity = pc.Inventory != null ? pc.Inventory.StockQuantity : 0,
                                ImageUrls = pc.ProductImages.Select(pi => pi.ImageUrl).ToList()
                            }).ToList()
                        }
                    })
                    .ToListAsync(ct);

                await _cache.SetAsync(cacheKey, featuredProducts, TimeSpan.FromHours(24));
                return Result<List<ResFeaturedProductDto>>.Success(featuredProducts);
            }
            catch (Exception ex)
            {
                return Result<List<ResFeaturedProductDto>>.Failure($"Failed to retrieve featured products: {ex.Message}", 500);
            }
        }
    }
}
