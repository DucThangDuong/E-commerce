using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries
{
    public record GetFilteredProductsQuery(List<int>? CategoryIds, List<int>? BrandIds, int Skip, int Take) : IRequest<Result<List<ResProductDto>>>;

    public class GetFilteredProductsHandler : IRequestHandler<GetFilteredProductsQuery, Result<List<ResProductDto>>>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetFilteredProductsHandler(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Result<List<ResProductDto>>> Handle(GetFilteredProductsQuery query, CancellationToken ct)
        {
            try
            {
                string cacheKey = $"products_{string.Join("_", query.CategoryIds ?? new List<int>())}" +
                    $"_{string.Join("_", query.BrandIds ?? new List<int>())}_{query.Skip}_{query.Take}";
                var cachedData = await _cache.GetAsync<List<ResProductDto>>(cacheKey);
                if (cachedData != null)
                {
                    return Result<List<ResProductDto>>.Success(cachedData);
                }

                var dbQuery = _db.Products.AsNoTracking().AsQueryable();

                if (query.CategoryIds != null && query.CategoryIds.Any())
                {
                    dbQuery = dbQuery.Where(p => query.CategoryIds.Contains(p.CategoryId));
                }

                if (query.BrandIds != null && query.BrandIds.Any())
                {
                    dbQuery = dbQuery.Where(p => p.BrandId.HasValue && query.BrandIds.Contains(p.BrandId.Value));
                }

                var result = await dbQuery
                    .OrderBy(e => e.ProductId)
                    .Skip(query.Skip)
                    .Take(query.Take)
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

                if (result != null)
                {
                    await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
                }
                return Result<List<ResProductDto>>.Success(result ?? new List<ResProductDto>());
            }
            catch (Exception ex)
            {
                return Result<List<ResProductDto>>.Failure(ex.Message, 400);
            }
        }
    }
}
