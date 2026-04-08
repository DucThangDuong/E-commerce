using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Brands.Queries
{
    public record GetAllBrandsQuery(int Take) : IRequest<Result<List<ResBrandDto>>>;

    public class GetAllBrandsHandler : IRequestHandler<GetAllBrandsQuery, Result<List<ResBrandDto>>>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;
        
        public GetAllBrandsHandler(ICacheService cache, IAppReadDbContext db)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Result<List<ResBrandDto>>> Handle(GetAllBrandsQuery query, CancellationToken ct)
        {
            try
            {
                string cacheKey = $"brand";
                var cachedData = await _cache.GetAsync<List<ResBrandDto>>(cacheKey);
                if(cachedData != null)
                {
                    return Result<List<ResBrandDto>>.Success(cachedData, 200);
                }
                
                var result = await _db.Brands
                    .AsNoTracking()
                    .OrderBy(e => e.BrandId)
                    .Take(query.Take)
                    .Select(b => new ResBrandDto
                    {
                        BrandId = b.BrandId,
                        Description = b.Description,
                        Name = b.Name,
                        LogoUrl = b.LogoUrl
                    })
                    .ToListAsync(ct);

                await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(24));
                return Result<List<ResBrandDto>>.Success(result, 200);
            }
            catch (Exception ex)
            {
                return Result<List<ResBrandDto>>.Failure(ex.Message, 500);
            }
        }
    }
}
