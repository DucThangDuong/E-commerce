using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly EcommerceOrderSystemContext _context;

        public BrandRepository(EcommerceOrderSystemContext context)
        {
            _context = context;
        }

        public async Task<Brand> AddNewBrandAsync(string name, string description, string? logoUrl)
        {
            var brand = new Brand
            {
                Name = name,
                Description = description,
                LogoUrl = logoUrl
            };
            await _context.Brands.AddAsync(brand);
            return brand;
        }



        public async Task<bool> BrandExistsAsync(int brandId, CancellationToken ct = default)
        {
            return await _context.Brands.AnyAsync(b => b.BrandId == brandId, ct);
        }
    }
}
