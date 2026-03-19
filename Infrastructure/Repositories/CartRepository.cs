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

        public async Task<Cart?> GetCartAsync(int customerId, int productId)
        {
            return await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);
        }

        public async Task<List<ResCartDto>> GetCartItemsByCustomerIdAsync(int customerId, CancellationToken ct = default)
        {
            return await _context.Carts
                .AsNoTracking()
                .Where(e => e.CustomerId == customerId)
                .Select(e => new ResCartDto
                {
                    BasePrice = e.Product.BasePrice,
                    CartId = e.CartId,
                    CategoryId = e.Product.CategoryId,
                    Description = e.Product.Description,
                    Name = e.Product.Name,
                    ProductId = e.Product.ProductId,
                    Quantity = e.Quantity,
                    StockQuantity = e.Product.Inventory != null ? e.Product.Inventory.StockQuantity : 0,
                    imageUrl = e.Product.ProductImages.Select(pi => pi.ImageUrl).ToList(),
                })
                .ToListAsync(ct);
        }
    }
}
