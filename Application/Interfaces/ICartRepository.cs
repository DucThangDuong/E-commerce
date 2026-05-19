using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICartRepository
    {
        Task<bool> AddNewCartAsync(Cart newCart);
        Task<Cart?> GetCartAsync(int customerId, int colorId);
        Task<bool> DeleteCartAsync(int customerId, int colorId);
        Task<bool> DeleteCartItemsAsync(int customerId, List<int> colorIds, CancellationToken ct = default);
    }
}
