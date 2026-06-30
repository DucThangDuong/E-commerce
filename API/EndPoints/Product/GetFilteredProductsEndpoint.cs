using API.Extensions;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace API.EndPoints.Product
{
    public class ReqGetFilteredProductsDto
    {
        [QueryParam]
        public List<int>? CategoryIds { get; set; }
        
        [QueryParam]
        public List<int>? BrandIds { get; set; }
        
        [QueryParam]
        public string? Keyword { get; set; }

        [QueryParam]
        public decimal? MinPrice { get; set; }

        [QueryParam]
        public decimal? MaxPrice { get; set; }

        [QueryParam]
        public int take { get; set; } = 10;
        
        [QueryParam]
        public int skip { get; set; } = 0;
    }

    public class GetFilteredProductsEndpoint : Endpoint<ReqGetFilteredProductsDto>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetFilteredProductsEndpoint(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public override void Configure()
        {
            Get("/product/filter");
            AllowAnonymous();
            Options(x => x.RequireRateLimiting("search_strict"));
        }

        public override async Task HandleAsync(ReqGetFilteredProductsDto req, CancellationToken ct)
        {
            try
            {
                string cacheKey = $"products_{string.Join("_", req.CategoryIds ?? new List<int>())}_{string.Join("_", req.BrandIds ?? new List<int>())}";

                var allProducts = await _cache.GetOrSetAsync(cacheKey, async () => 
                {
                    var dbQuery = _db.Products.AsNoTracking().AsQueryable();

                    if (req.CategoryIds != null && req.CategoryIds.Any())
                    {
                        dbQuery = dbQuery.Where(p => req.CategoryIds.Contains(p.CategoryId));
                    }

                    if (req.BrandIds != null && req.BrandIds.Any())
                    {
                        dbQuery = dbQuery.Where(p => p.BrandId.HasValue && req.BrandIds.Contains(p.BrandId.Value));
                    }

                    return await dbQuery
                        .Select(e => new ResProductDto
                        {
                            BasePrice = e.BasePrice,
                            DiscountedPrice = e.BasePrice - (e.Promotions
                                .Where(p => p.IsActive == true && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                                .Select(p => p.DiscountType.ToLower().Contains("percent") 
                                    ? (e.BasePrice * p.DiscountValue / 100M) 
                                    : p.DiscountValue)
                                .OrderByDescending(x => x)
                                .FirstOrDefault()),
                            AppliedPromotion = e.Promotions
                                .Where(p => p.IsActive == true && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                                .Select(p => new ResProductPromotionDto
                                {
                                    PromotionName = p.Name,
                                    DiscountType = p.DiscountType,
                                    DiscountValue = p.DiscountValue,
                                    AmountReduced = p.DiscountType.ToLower().Contains("percent") 
                                        ? (e.BasePrice * p.DiscountValue / 100M) 
                                        : p.DiscountValue
                                })
                                .OrderByDescending(p => p.AmountReduced)
                                .FirstOrDefault(),
                            CategoryId = e.CategoryId,
                            BrandId = e.BrandId,
                            Description = e.Description,
                            Name = e.Name,
                            ProductId = e.ProductId,
                            ImageUrls = e.ProductImages.Where(pi => pi.ColorId == null).Select(pi => pi.ImageUrl).ToList(),
                            Colors = e.ProductColors.Select(pc => new ResProductColorDto
                            {
                                ColorId = pc.ColorId,
                                ColorName = pc.ColorName,
                                PriceAdjustment = pc.PriceAdjustment,
                                StockQuantity = pc.Vehicles.Count(v => v.Status == "Available"),
                                ImageUrls = pc.ProductImages.Select(pi => pi.ImageUrl).ToList()
                            }).ToList()
                        })
                        .ToListAsync(ct);
                }, TimeSpan.FromMinutes(10));

                var filteredProducts = allProducts?.AsEnumerable() ?? Enumerable.Empty<ResProductDto>();

                if (!string.IsNullOrWhiteSpace(req.Keyword))
                {
                    filteredProducts = filteredProducts.Where(p => p.Name.Contains(req.Keyword, StringComparison.OrdinalIgnoreCase));
                }

                if (req.MinPrice.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.BasePrice >= req.MinPrice.Value);
                }

                if (req.MaxPrice.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.BasePrice <= req.MaxPrice.Value);
                }

                int totalItems = filteredProducts.Count();
                int take = req.take > 0 ? req.take : 10;

                var pagedProducts = filteredProducts
                    .OrderBy(p => p.ProductId)
                    .Skip(req.skip)
                    .Take(take)
                    .ToList();

                var result = new ResPagedProductDto
                {
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)take),
                    CurrentPage = (req.skip / take) + 1,
                    PageSize = take,
                    Products = pagedProducts
                };

                await this.SendApiResponseAsync(Application.Common.Result<ResPagedProductDto>.Success(result), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Application.Common.Result<ResPagedProductDto>.Failure(ex.Message, 400), ct);
            }
        }
    }
}
