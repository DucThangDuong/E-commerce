using API.Extensions;
using Application.DTOs.Response;
using Application.Common;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace API.EndPoints.Brand
{
    public class ReqGetBrandDto
    {
        public int take { get; set; } = 10;
        public int skip { get; set; } = 0;
    }

    public class GetAllBrandsEndpoint : Endpoint<ReqGetBrandDto>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;
        
        public GetAllBrandsEndpoint(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public override void Configure()
        {
            Get("/brand");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ReqGetBrandDto req, CancellationToken ct)
        {
            try
            {
                string cacheKey = $"brand";

                var cachedResult = await _cache.GetOrSetAsync(cacheKey, async () => 
                {
                    return await _db.Brands
                        .AsNoTracking()
                        .OrderBy(e => e.BrandId)
                        .Take(req.take)
                        .Select(b => new ResBrandDto
                        {
                            BrandId = b.BrandId,
                            Description = b.Description,
                            Name = b.Name,
                            LogoUrl = b.LogoUrl
                        })
                        .ToListAsync(ct);
                }, TimeSpan.FromHours(24));

                await this.SendApiResponseAsync(Result<List<ResBrandDto>>.Success(cachedResult ?? new List<ResBrandDto>(), 200), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Result<List<ResBrandDto>>.Failure(ex.Message, 500), ct);
            }
        }
    }
}
