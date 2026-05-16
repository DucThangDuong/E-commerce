using Application.IServices;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services
{
    public class NotificationHub : Hub
    {
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
        }
    }
}
