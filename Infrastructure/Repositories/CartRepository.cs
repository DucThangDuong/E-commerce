using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly EcommerceContext _context;

        public CartRepository(EcommerceContext context)
        {
            _context = context;
        }

        public async Task<bool> AddNewCartAsync(Cart newCart)
        {
            await _context.Carts.AddAsync(newCart);
            return true;
        }

        public async Task<bool> DeleteCartAsync(int customerId, int colorId)
        {
            var deleted = await _context.Carts
                .Where(e => e.ColorId == colorId && e.CustomerId == customerId)
                .ExecuteDeleteAsync();
            return deleted > 0;
        }

        public async Task<bool> DeleteCartItemsAsync(int customerId, List<int> colorIds, CancellationToken ct = default)
        {
            var deleted = await _context.Carts
                .Where(e => e.CustomerId == customerId && colorIds.Contains(e.ColorId))
                .ExecuteDeleteAsync(ct);
            return deleted > 0;
        }

        public async Task<Cart?> GetCartAsync(int customerId, int colorId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ColorId == colorId);
        }

        public async Task<Cart?> GetCartByIdAsync(int cartId, int customerId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.CustomerId == customerId);
        }

        public async Task<bool> DeleteCartByIdAsync(int cartId, int customerId)
        {
            var deleted = await _context.Carts
                .Where(e => e.CartId == cartId && e.CustomerId == customerId)
                .ExecuteDeleteAsync();
            return deleted > 0;
        }


    }
}
