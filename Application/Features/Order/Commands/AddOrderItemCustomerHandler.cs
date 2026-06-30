using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using StackExchange.Redis;

namespace Application.Features.Order.Commands
{

    public record ValidateCartCommand(int CustomerId, List<CartItemRequest> Items) : IRequest<Result<ValidateCartResponse>>;

    public class ValidateCartHandler : IRequestHandler<ValidateCartCommand, Result<ValidateCartResponse>>
    {
        private readonly IDatabase _redisConnection;

        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        public ValidateCartHandler(IConnectionMultiplexer multiplexer, IInventoryRepository inventoryRepository, IProductRepository productRepository)
        {
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _redisConnection = multiplexer.GetDatabase();
        }

        public async Task<Result<ValidateCartResponse>> Handle(ValidateCartCommand request, CancellationToken ct)
        {
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

            var stockItemShopHave = new Dictionary<int, int>();
            var missingColorIds = new List<int>();

            var redisKeys = colorIds.Select(id => (RedisKey)$"Color:Stock:{id}").ToArray();
            var redisValues = await _redisConnection.StringGetAsync(redisKeys);

            for (int i = 0; i < colorIds.Count; i++)
            {
                if (redisValues[i].HasValue && int.TryParse(redisValues[i], out int redisStock))
                {
                    stockItemShopHave[colorIds[i]] = redisStock;
                }
                else
                {
                    missingColorIds.Add(colorIds[i]);
                }
            }

            if (missingColorIds.Any())
            {
                var dbStockMap = await _inventoryRepository.GetStockByColorIdsAsync(missingColorIds, ct);
                foreach (var id in missingColorIds)
                {
                    int dbStock = dbStockMap.ContainsKey(id) ? dbStockMap[id] : 0;
                    stockItemShopHave[id] = dbStock;
                    await _redisConnection.StringSetAsync($"Color:Stock:{id}", dbStock, TimeSpan.FromDays(1));
                }
            }

            var outOfStockItems = new List<ValidatedCartItem>();
            foreach (var item in itemUserWantBuy)
            {
                int stock = stockItemShopHave.ContainsKey(item.Key) ? stockItemShopHave[item.Key] : 0;
                if (stock < item.Value)
                {
                    outOfStockItems.Add(new ValidatedCartItem
                    {
                        ColorId = item.Key,
                        Quantity = item.Value,
                        AvailableStock = stock
                    });
                }
            }

            if (outOfStockItems.Any())
            {
                return Result<ValidateCartResponse>.Failure("Các sản phẩm không đủ tồn kho.", 400);
            }

            Dictionary<int, decimal> colorPrices = await _productRepository.GetPricesByColorIdsAsync(colorIds, ct);

            var validatedItems = new List<ValidatedCartItem>();
            decimal subTotal = 0;

            foreach (var item in itemUserWantBuy)
            {
                if (!colorPrices.TryGetValue(item.Key, out decimal unitPrice))
                {
                    return Result<ValidateCartResponse>.Failure($"Sản phẩm màu ID {item.Key} không có thông tin giá hợp lệ.", 400);
                }

                decimal lineTotal = unitPrice * item.Value;
                validatedItems.Add(new ValidatedCartItem
                {
                    ColorId = item.Key,
                    Quantity = item.Value,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal,
                    AvailableStock = stockItemShopHave.ContainsKey(item.Key) ? stockItemShopHave[item.Key] : 0
                });
                subTotal += lineTotal;
            }

            return Result<ValidateCartResponse>.Success(new ValidateCartResponse
            {
                SubTotal = subTotal,
                Items = validatedItems
            }, 200);
        }
    }
}
