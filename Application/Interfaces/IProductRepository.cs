using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task<int?> GetStockQuantityAsync(int productId, CancellationToken ct = default);
        Task AddFeaturedProductAsync(FeaturedProduct featuredProduct, CancellationToken ct = default);
        Task<bool> ProductExistsAsync(int productId, CancellationToken ct = default);
        Task<Dictionary<int, decimal>> GetProductPricesAsync(List<int> productIds, CancellationToken ct = default);
    }
}
