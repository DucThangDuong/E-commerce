using Application.Common;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using MassTransit;
using MediatR;
using StackExchange.Redis;
using System.Text.Json;

namespace Application.Features.Order.Commands
{
    public record AddOrderItemCustomerCommand(int CustomerId, Dictionary<int,int> Items) : IRequest<Result<string>>;
    public class AddOrderItemCustomerHandler : IRequestHandler<AddOrderItemCustomerCommand, Result<string>>
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly INotificationService _hubContext;
        private readonly IDatabase _redisConnection;
        private readonly IPublishEndpoint _publishEndpoint;

        public AddOrderItemCustomerHandler(IUnitOfWork unitOfWork, INotificationService hub, IConnectionMultiplexer multiplexer, IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hub;
            _redisConnection = multiplexer.GetDatabase();
            _publishEndpoint = publishEndpoint;
        }
        public async Task<Result<string>> Handle(AddOrderItemCustomerCommand request, CancellationToken ct)
        {
            List<int> colorIds = request.Items.Keys.ToList();
            var stockMap = new Dictionary<int, int>();
            var missingColorIds = new List<int>();

            var redisKeys = colorIds.Select(id => (RedisKey)$"Color:Stock:{id}").ToArray();
            var redisValues = await _redisConnection.StringGetAsync(redisKeys);

            for (int i = 0; i < colorIds.Count; i++)
            {
                if (redisValues[i].HasValue && int.TryParse(redisValues[i], out int redisStock))
                {
                    stockMap[colorIds[i]] = redisStock;
                }
                else
                {
                    missingColorIds.Add(colorIds[i]);
                }
            }

            if (missingColorIds.Any())
            {
                Dictionary<int,int> dbStockMap = await _unitOfWork.InventoryRepository.GetStockByColorIdsAsync(missingColorIds, ct);
                
                foreach (var id in missingColorIds)
                {
                    int dbStock = dbStockMap.ContainsKey(id) ? dbStockMap[id] : 0;
                    stockMap[id] = dbStock;
                    await _redisConnection.StringSetAsync($"Color:Stock:{id}", dbStock, TimeSpan.FromDays(1));
                }
            }

            var outOfStockItems = new List<int>();
            foreach (var item in request.Items)
            {
                int stock = stockMap.ContainsKey(item.Key) ? stockMap[item.Key] : 0;
                if (stock < item.Value)
                {
                    outOfStockItems.Add(item.Key);
                }
            }

            if (outOfStockItems.Any())
            {
                return Result<string>.Failure("Các sản phẩm không đủ tồn kho.", 400);
            }

            var successItems = new Dictionary<int, int>();
            
            try
            {
                foreach (var item in request.Items)
                {
                    string cacheKeyStock = $"Color:Stock:{item.Key}";
                    long newStock = await _redisConnection.StringDecrementAsync(cacheKeyStock, item.Value);
                    if (newStock < 0)
                    {
                        await _redisConnection.StringIncrementAsync(cacheKeyStock, item.Value);
                        
                        foreach (var success in successItems)
                        {
                            await _redisConnection.StringIncrementAsync($"Color:Stock:{success.Key}", success.Value);
                        }
                        
                        return Result<string>.Failure("Sản phẩm đã hết hàng do có người khác vừa mua.", 400);
                    }
                    successItems.Add(item.Key, item.Value);
                }

                string reservationId = Guid.NewGuid().ToString("N");
                string reservationKey = $"Order:Reservation:{reservationId}";
                var reservationData = new
                {
                    CustomerId = request.CustomerId,
                    Items = request.Items
                };
                
                await _redisConnection.StringSetAsync(reservationKey, JsonSerializer.Serialize(reservationData), TimeSpan.FromMinutes(15));
                await _publishEndpoint.Publish(new DTOs.Services.ReserveOrderEvent(reservationId), context =>
                {
                    context.Delay = TimeSpan.FromMinutes(15);
                });

                return Result<string>.Success(reservationId, 201);
            }
            catch (Exception)
            {
                foreach (var success in successItems)
                {
                    string cacheKeyStock = $"Color:Stock:{success.Key}";
                    await _redisConnection.StringIncrementAsync(cacheKeyStock, success.Value);
                }
                throw;
            }

        }
    }
}
