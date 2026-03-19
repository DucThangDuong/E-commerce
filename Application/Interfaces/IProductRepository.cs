using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task<int?> GetStockQuantityAsync(int productId, CancellationToken ct = default);
        Task<List<ResProductDto>> GetAllProductsAsync(int skip, int take, CancellationToken ct = default);
        Task<ResProductDto?> GetProductDetailAsync(int productId, CancellationToken ct = default);
        Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default);
    }
}
