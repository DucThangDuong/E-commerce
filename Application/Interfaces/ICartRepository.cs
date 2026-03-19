using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICartRepository
    {
        Task<bool> AddNewCartAsync(Cart newCart);
        Task<Cart?> GetCartAsync(int customerId, int productId);
        Task<bool> DeleteCartAsync(int customerId, int productId);
        Task<List<ResCartDto>> GetCartItemsByCustomerIdAsync(int customerId, CancellationToken ct = default);
    }
}
