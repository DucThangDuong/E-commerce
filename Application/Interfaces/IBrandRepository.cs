using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBrandRepository
    {
        Task<Brand> AddNewBrandAsync(string name, string description, string? logoUrl);
        Task<bool> BrandExistsAsync(int brandId, CancellationToken ct = default);
    }
}
