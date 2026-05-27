using Application.DTOs.Services;
using Application.Interfaces;
using MassTransit;
using StackExchange.Redis;
using System.Text.Json;

namespace Application.Consumers
{
    public class CheckOrderExpirationConsumer : IConsumer<ReserveOrderEvent>
    {
        private readonly IDatabase _redisConnection;

        public CheckOrderExpirationConsumer(IConnectionMultiplexer multiplexer)
        {
            _redisConnection = multiplexer.GetDatabase();
        }

        public async Task Consume(ConsumeContext<ReserveOrderEvent> context)
        {
            string reservationId = context.Message.ReservationId;
            string reservationKey = $"Order:Reservation:{reservationId}";
            var reservationValue = await _redisConnection.StringGetAsync(reservationKey);

            if (!reservationValue.HasValue)
            {
                return;
            }

            var reservationData = JsonSerializer.Deserialize<JsonElement>(reservationValue!);
            var itemsElement = reservationData.GetProperty("Items");
            var items = JsonSerializer.Deserialize<Dictionary<int, int>>(itemsElement.GetRawText());

            if (items != null)
            {
                foreach (var item in items)
                {
                    string cacheKeyStock = $"Product:Stock:{item.Key}";
                    await _redisConnection.StringIncrementAsync(cacheKeyStock, item.Value);
                }
            }

            await _redisConnection.KeyDeleteAsync(reservationKey);
        }
    }
}
