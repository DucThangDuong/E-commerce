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

        public async Task<List<ResProductDto>> GetAllProductsAsync(int skip, int take, CancellationToken ct = default)
        {
            return await _context.Products
                .AsNoTracking()
                .OrderBy(e => e.ProductId)
                .Skip(skip)
                .Take(take)
                .Select(e => new ResProductDto
                {
                    BasePrice = e.BasePrice,
                    CategoryId = e.CategoryId,
                    Description = e.Description,
                    Name = e.Name,
                    ProductId = e.ProductId,
                    StockQuantity = e.Inventory != null ? e.Inventory.StockQuantity : 0,
                    imageUrl = e.ProductImages.Select(pi => pi.ImageUrl).ToList(),
                })
                .ToListAsync(ct);
        }

        public async Task<ResProductDto?> GetProductDetailAsync(int productId, CancellationToken ct = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(e => e.ProductId == productId)
                .Select(e => new ResProductDto
                {
                    BasePrice = e.BasePrice,
                    CategoryId = e.CategoryId,
                    Description = e.Description,
                    Name = e.Name,
                    ProductId = e.ProductId,
                    StockQuantity = e.Inventory != null ? e.Inventory.StockQuantity : 0,
                    imageUrl = e.ProductImages.Select(pi => pi.ImageUrl).ToList(),
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryId == categoryId, ct);
        }
    }
}
