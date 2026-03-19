using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly EcommerceOrderSystemContext _context;

        public CategoryRepository(EcommerceOrderSystemContext context)
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

        public async Task<List<ResCategoryDto>> GetAllCategoriesAsync(int take, CancellationToken ct = default)
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(e => e.CategoryId)
                .Take(take)
                .Select(c => new ResCategoryDto
                {
                    CategoryId = c.CategoryId,
                    Description = c.Description,
                    Name = c.Name,
                    Picture = c.Picture
                })
                .ToListAsync(ct);
        }
    }
}
