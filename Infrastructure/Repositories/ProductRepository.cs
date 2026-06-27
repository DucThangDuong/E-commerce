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
            return await _context.Vehicles
                .Where(v => v.Color.ProductId == productId && v.Status == "Available")
                .CountAsync(ct);
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
            var data = await _context.ProductColors
                .AsNoTracking()
                .Where(pc => colorIds.Contains(pc.ColorId))
                .Select(pc => new 
                {
                    pc.ColorId,
                    BasePrice = pc.Product.BasePrice,
                    PriceAdjustment = pc.PriceAdjustment ?? 0,
                    BestDiscount = pc.Product.Promotions
                        .Where(p => p.IsActive == true && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                        .Select(p => p.DiscountType.ToLower().Contains("percent") 
                            ? (pc.Product.BasePrice * p.DiscountValue / 100M) 
                            : p.DiscountValue)
                        .OrderByDescending(x => x)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            return data.ToDictionary(
                x => x.ColorId, 
                x => (x.BasePrice - x.BestDiscount) + x.PriceAdjustment
            );
        }
    }
}
