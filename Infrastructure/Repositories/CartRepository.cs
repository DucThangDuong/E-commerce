using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly EcommerceOrderSystemContext _context;

        public CartRepository(EcommerceOrderSystemContext context)
        {
            _context = context;
        }

        public async Task<bool> AddNewCartAsync(Cart newCart)
        {
            await _context.Carts.AddAsync(newCart);
            return true;
        }

        public async Task<bool> DeleteCartAsync(int customerId, int productId)
        {
            var deleted = await _context.Carts
                .Where(e => e.ProductId == productId && e.CustomerId == customerId)
                .ExecuteDeleteAsync();
            return deleted > 0;
        }

        public async Task<bool> DeleteCartItemsAsync(int customerId, List<int> productIds, CancellationToken ct = default)
        {
            var deleted = await _context.Carts
                .Where(e => e.CustomerId == customerId && productIds.Contains(e.ProductId))
                .ExecuteDeleteAsync(ct);
            return deleted > 0;
        }

        public async Task<Cart?> GetCartAsync(int customerId, int productId)
        {
            return await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);
        }


    }
}
