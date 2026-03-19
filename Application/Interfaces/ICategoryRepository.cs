using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category> AddNewCategoryAsync(string name, string description, string? picture);
        Task<List<ResCategoryDto>> GetAllCategoriesAsync(int take, CancellationToken ct = default);
    }
}
