using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category> AddNewCategoryAsync(string name, string description, string? picture);
        Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default);
    }
}
