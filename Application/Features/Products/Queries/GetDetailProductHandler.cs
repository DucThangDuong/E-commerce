using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace Application.Features.Products.Queries
{
    public record GetDetailProductQuery(int ProductId) : IRequest<Result<ResProductDto>>;

    public class GetDetailProductHandler : IRequestHandler<GetDetailProductQuery, Result<ResProductDto>>
    {
        private readonly IAppReadDbContext _db;
        private readonly IDatabase _redisConnection;
        private readonly ICacheService _cache;

        public GetDetailProductHandler(IAppReadDbContext db, IConnectionMultiplexer redisConnection, ICacheService cache)
        {
            _db = db;
            _redisConnection = redisConnection.GetDatabase();
            _cache = cache;
        }

        public async Task<Result<ResProductDto>> Handle(GetDetailProductQuery query, CancellationToken ct)
        {
            try
            {
                string cacheKeyInfo = $"Product:Detail:{query.ProductId}";
                
                var product = await _cache.GetOrSetAsync(cacheKeyInfo, async () =>
                {
                    return await _db.Products
                        .AsNoTracking()
                        .Where(e => e.ProductId == query.ProductId)
                        .Select(e => new ResProductDto
                        {
                            BasePrice = e.BasePrice,
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
                                StockQuantity = pc.Inventory != null ? pc.Inventory.StockQuantity : 0,
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
                    return Result<ResProductDto>.Failure("Product not found", 404);
                }

                // Luôn cập nhật tồn kho theo thời gian thực từ Redis bằng MGET
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
                            // Nếu trong redis chưa có (trường hợp hiếm gặp), ta set lại giá trị mặc định từ DB vào Redis để dùng sau này
                            await _redisConnection.StringSetAsync(redisKeys[i], product.Colors[i].StockQuantity, TimeSpan.FromDays(1), When.NotExists);
                        }
                    }
                }

                return Result<ResProductDto>.Success(product, 200);
            }
            catch (Exception ex)
            {
                return Result<ResProductDto>.Failure($"Lỗi khi lấy chi tiết sản phẩm: {ex.Message}", 500);
            }
        }
    }
}
