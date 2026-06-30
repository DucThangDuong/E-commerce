using API.Extensions;
using Application.DTOs.Response;
using Application.Common;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace API.EndPoints.Category
{
    public class ReqGetTotalCategoryDto
    {
        public int take { get; set; } = 10;
    }

    public class GetTotalCategoryEndpoint : Endpoint<ReqGetTotalCategoryDto, List<ResCategoryDto>>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;
        
        public GetTotalCategoryEndpoint(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public override void Configure()
        {
            Get("/category");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ReqGetTotalCategoryDto req, CancellationToken ct)
        {
            try
            {
                string cacheKey = $"category";

                var cachedResult = await _cache.GetOrSetAsync(cacheKey, async () => 
                {
                    return await _db.Categories
                        .AsNoTracking()
                        .OrderBy(e => e.CategoryId)
                        .Take(req.take)
                        .Select(c => new ResCategoryDto
                        {
                            CategoryId = c.CategoryId,
                            Description = c.Description,
                            Name = c.Name,
                            Picture = c.Picture
                        })
                        .ToListAsync(ct);
                }, TimeSpan.FromHours(24));

                await this.SendApiResponseAsync(Result<List<ResCategoryDto>>.Success(cachedResult ?? new List<ResCategoryDto>()), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Result<List<ResCategoryDto>>.Failure(ex.Message), ct);
            }
        }
    }
}