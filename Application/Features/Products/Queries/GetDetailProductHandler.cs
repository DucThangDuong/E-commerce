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
    public record GetDetailProductQuery(int ProductId, string? ConnectionId = null) : IRequest<Result<ResProductDto>>;

    public class GetDetailProductHandler : IRequestHandler<GetDetailProductQuery, Result<ResProductDto>>
    {
        private readonly IAppReadDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly IDatabase _redisConnection;
        public GetDetailProductHandler(IAppReadDbContext db, INotificationService notificationService, IConnectionMultiplexer redisConnection)
        {
            _db = db;
            _notificationService = notificationService;
            _redisConnection = redisConnection.GetDatabase();
        }

        public async Task<Result<ResProductDto>> Handle(GetDetailProductQuery query, CancellationToken ct)
        {
            try
            {
                string cacheKeyInfo = $"Product:Detail:{query.ProductId}";
                var infoTask = await _redisConnection.StringGetAsync(cacheKeyInfo);
                if (!infoTask.IsNullOrEmpty)
                {
                    var cachedProduct = JsonSerializer.Deserialize<ResProductDto>(infoTask.ToString());
                    if (cachedProduct != null && cachedProduct.Colors.Any())
                    {
                        var colorIds = cachedProduct.Colors.Select(c => c.ColorId).ToList();
                        var redisKeys = colorIds.Select(id => (RedisKey)$"Color:Stock:{id}").ToArray();
                        
                        var redisValues = await _redisConnection.StringGetAsync(redisKeys);

                        for (int i = 0; i < colorIds.Count; i++)
                        {
                            if (redisValues[i].HasValue && int.TryParse(redisValues[i], out int redisStock))
                            {
                                cachedProduct.Colors[i].StockQuantity = redisStock;
                            }
                        }
                        
                        return Result<ResProductDto>.Success(cachedProduct);
                    }
                }
                var product = await _db.Products
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
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(ct);

                if (product == null)
                {
                    return Result<ResProductDto>.Failure("Product not found", 404);
                }
                if (!string.IsNullOrWhiteSpace(query.ConnectionId))
                {
                    await _notificationService.AddConnectionToGroup(query.ConnectionId, $"Product_{query.ProductId}");
                }
                await _redisConnection.StringSetAsync(cacheKeyInfo, JsonSerializer.Serialize(product), TimeSpan.FromMinutes(10));
                var tasks = new List<Task>();
                foreach (var i in product.Colors)
                {
                    var stockColorId = $"Color:Stock:{i.ColorId}";
                    tasks.Add(_redisConnection.StringSetAsync(stockColorId, i.StockQuantity, TimeSpan.FromDays(1), When.NotExists));
                }
                await Task.WhenAll(tasks);
                if (product.Colors.Any())
                {
                    var colorIds = product.Colors.Select(c => c.ColorId).ToList();
                    var redisKeys = colorIds.Select(id => (RedisKey)$"Color:Stock:{id}").ToArray();
                    var redisValues = await _redisConnection.StringGetAsync(redisKeys);

                    for (int i = 0; i < colorIds.Count; i++)
                    {
                        if (redisValues[i].HasValue && int.TryParse(redisValues[i], out int redisStock))
                        {
                            product.Colors[i].StockQuantity = redisStock;
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
