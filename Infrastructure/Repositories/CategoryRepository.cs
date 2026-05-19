using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly EcommerceContext _context;

        public CategoryRepository(EcommerceContext context)
        {
            _context = context;
        }

        public async Task<Category> AddNewCategoryAsync(string name, string description, string? picture)
        {
            var category = new Category
            {
                Name = name,
                Description = description,
                Picture = picture
            };
            await _context.Categories.AddAsync(category);
            return category;
        }


        public async Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryId == categoryId, ct);
        }
    }
}
