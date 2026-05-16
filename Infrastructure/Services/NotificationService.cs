using Amazon.Runtime.Internal.Util;
using Application.IServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;
        public NotificationService(IHubContext<NotificationHub> hubContext , ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendProductUpdateNotification(int productId, string message)
        {
            string groupName = $"Product_{productId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveProductUpdate", message);
        }

        public async Task SendMessageToOrderId(string orderId, string message)
        {
            await _hubContext.Clients.Group(orderId).SendAsync("ReceiveOrderIdMessage", message);
        }

        public async Task AddConnectionToGroup(string connectionId, string groupName)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
            _logger.LogInformation($"Connection {connectionId} added to group {groupName}");
        }
    }
}
