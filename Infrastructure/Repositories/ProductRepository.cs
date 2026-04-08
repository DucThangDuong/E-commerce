using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcommerceOrderSystemContext _context;

        public ProductRepository(EcommerceOrderSystemContext context)
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
                .Where(i => i.ProductId == productId)
                .Select(i => (int?)i.StockQuantity)
                .FirstOrDefaultAsync(ct);
        }




        public async Task AddFeaturedProductAsync(FeaturedProduct featuredProduct, CancellationToken ct = default)
        {
            await _context.FeaturedProducts.AddAsync(featuredProduct, ct);
        }



        public async Task<bool> ProductExistsAsync(int productId, CancellationToken ct = default)
        {
            return await _context.Products.AnyAsync(e=>e.ProductId == productId);
        }


        public async Task<Dictionary<int, decimal>> GetProductPricesAsync(List<int> productIds, CancellationToken ct = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, p => p.BasePrice, ct);
        }
    }
}
