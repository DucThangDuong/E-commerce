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
                    BrandId = e.BrandId,
                    Description = e.Description,
                    Name = e.Name,
                    ProductId = e.ProductId,
                    StockQuantity = e.Inventory != null ? e.Inventory.StockQuantity : 0,
                    imageUrl = e.ProductImages.Select(pi => pi.ImageUrl).ToList(),
                })
                .ToListAsync(ct);
        }

        public async Task<List<ResProductDto>> GetFilteredProductsAsync(List<int>? categoryIds, List<int>? brandIds, int skip, int take, CancellationToken ct = default)
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (brandIds != null && brandIds.Any())
            {
                query = query.Where(p => p.BrandId.HasValue && brandIds.Contains(p.BrandId.Value));
            }

            return await query
                .OrderBy(e => e.ProductId)
                .Skip(skip)
                .Take(take)
                .Select(e => new ResProductDto
                {
                    BasePrice = e.BasePrice,
                    CategoryId = e.CategoryId,
                    BrandId = e.BrandId,
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
                    BrandId = e.BrandId,
                    Description = e.Description,
                    Name = e.Name,
                    ProductId = e.ProductId,
                    StockQuantity = e.Inventory != null ? e.Inventory.StockQuantity : 0,
                    imageUrl = e.ProductImages.Select(pi => pi.ImageUrl).ToList(),
                })
                .FirstOrDefaultAsync(ct);
        }


        public async Task AddFeaturedProductAsync(FeaturedProduct featuredProduct, CancellationToken ct = default)
        {
            await _context.FeaturedProducts.AddAsync(featuredProduct, ct);
        }

        public async Task<List<ResFeaturedProductDto>> GetFeaturedProductsAsync(CancellationToken ct = default)
        {
            return await _context.FeaturedProducts
                .AsNoTracking()
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new ResFeaturedProductDto
                {
                    FeaturedId = f.FeaturedId,
                    ProductId = f.ProductId,
                    DisplayOrder = f.DisplayOrder,
                    StartDate = f.StartDate,
                    EndDate = f.EndDate,
                    Product = new ResProductDto
                    {
                        BasePrice = f.Product.BasePrice,
                        CategoryId = f.Product.CategoryId,
                        BrandId = f.Product.BrandId,
                        Description = f.Product.Description,
                        Name = f.Product.Name,
                        ProductId = f.Product.ProductId,
                        StockQuantity = f.Product.Inventory != null ? f.Product.Inventory.StockQuantity : 0,
                        imageUrl = f.Product.ProductImages.Select(pi => pi.ImageUrl).ToList(),
                    }
                })
                .ToListAsync(ct);
        }

        public async Task<bool> ProductExistsAsync(int productId, CancellationToken ct = default)
        {
            return await _context.Products.AnyAsync(e=>e.ProductId == productId);
        }
    }
}
