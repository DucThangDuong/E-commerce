using System.Threading.Tasks;

namespace Application.IServices
{
    public interface INotificationService
    {
        Task SendProductUpdateNotification(int productId, string message);
        Task SendMessageToOrderId(string orderId, string message);
        Task AddConnectionToGroup(string connectionId, string groupName);
    }
}
