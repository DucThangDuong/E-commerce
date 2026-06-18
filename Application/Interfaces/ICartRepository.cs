using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICartRepository
    {
        Task<bool> AddNewCartAsync(Cart newCart);
        Task<Cart?> GetCartAsync(int customerId, int colorId);
        Task<Cart?> GetCartByIdAsync(int cartId, int customerId);
        Task<bool> DeleteCartAsync(int customerId, int colorId);
        Task<bool> DeleteCartByIdAsync(int cartId, int customerId);
        Task<bool> DeleteCartItemsAsync(int customerId, List<int> colorIds, CancellationToken ct = default);
    }
}
