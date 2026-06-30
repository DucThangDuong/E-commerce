using API.Extensions;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace API.EndPoints.Product
{
    public class ReqGetDetalProductDto
    {
        public int productId { get; set; }
    }

    public class GetDetailProductEndpoint : Endpoint<ReqGetDetalProductDto>
    {
        private readonly IAppReadDbContext _db;
        private readonly IDatabase _redisConnection;
        private readonly ICacheService _cache;

        public GetDetailProductEndpoint(IAppReadDbContext db, IConnectionMultiplexer redisConnection, ICacheService cache)
        {
            _db = db;
            _redisConnection = redisConnection.GetDatabase();
            _cache = cache;
        }

        public override void Configure()
        {
            Get("/product/detail");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ReqGetDetalProductDto req, CancellationToken ct)
        {
            try
            {
                string cacheKeyInfo = $"Product:Detail:{req.productId}";
                
                var product = await _cache.GetOrSetAsync(cacheKeyInfo, async () =>
                {
                    return await _db.Products
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Where(e => e.ProductId == req.productId)
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
                            }).ToList(),
                            Specifications = e.ProductSpecifications.Select(s => new ResProductSpecificationDto
                            {
                                SpecName = s.Spec.SpecName,
                                SpecValue = s.SpecValue
                            }).ToList()
                        })
                        .FirstOrDefaultAsync(ct);
                }, TimeSpan.FromMinutes(30));

                if (product == null)
                {
                    await this.SendApiResponseAsync(Application.Common.Result<ResProductDto>.Failure("Product not found", 404), ct);
                    return;
                }

                if (product.Colors != null && product.Colors.Any())
                {
                    var colorIds = product.Colors.Select(c => c.ColorId).ToList();
                    var redisKeys = colorIds.Select(id => (RedisKey)$"Color:Stock:{id}").ToArray();
                    
                    var redisValues = await _redisConnection.StringGetAsync(redisKeys);

                    for (int i = 0; i < colorIds.Count; i++)
                    {
                        if (redisValues[i].HasValue && int.TryParse(redisValues[i], out int parsedStock))
                        {
                            product.Colors[i].StockQuantity = parsedStock;
                        }
                        else
                        {
                            await _redisConnection.StringSetAsync(redisKeys[i], product.Colors[i].StockQuantity, TimeSpan.FromDays(1), When.NotExists);
                        }
                    }
                }

                await this.SendApiResponseAsync(Application.Common.Result<ResProductDto>.Success(product, 200), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Application.Common.Result<ResProductDto>.Failure($"Lỗi khi lấy chi tiết sản phẩm: {ex.Message}", 500), ct);
            }
        }
    }
}
