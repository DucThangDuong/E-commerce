using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBrandRepository
    {
        Task<Brand> AddNewBrandAsync(string name, string description, string? logoUrl);
        Task<List<ResBrandDto>> GetAllBrandsAsync(int take, CancellationToken ct = default);
        Task<bool> BrandExistsAsync(int brandId, CancellationToken ct = default);
    }
}
