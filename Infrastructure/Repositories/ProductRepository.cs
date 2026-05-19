using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcommerceContext _context;

        public ProductRepository(EcommerceContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task<int?> GetStockQuantityAsync(int productId, CancellationToken ct = default)
        {
            return await _context.Inventories
                .Where(i => i.Color.ProductId == productId)
                .SumAsync(i => (int?)i.StockQuantity, ct);
        }
        public async Task AddFeaturedProductAsync(FeaturedProduct featuredProduct, CancellationToken ct = default)
        {
            await _context.FeaturedProducts.AddAsync(featuredProduct, ct);
        }

        public async Task<bool> ProductExistsAsync(int productId, CancellationToken ct = default)
        {
            return await _context.Products.AnyAsync(e=>e.ProductId == productId);
        }

        public async Task<Dictionary<int, decimal>> GetPricesByColorIdsAsync(List<int> colorIds, CancellationToken ct = default)
        {
            return await _context.ProductColors
                .Include(pc => pc.Product)
                .AsNoTracking()
                .Where(pc => colorIds.Contains(pc.ColorId))
                .ToDictionaryAsync(pc => pc.ColorId, pc => pc.Product.BasePrice + (pc.PriceAdjustment ?? 0), ct);
        }
    }
}
