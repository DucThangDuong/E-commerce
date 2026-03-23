using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Application.Features.Brands.Queries
{
    public record GetAllBrandsQuery(int Take) : IRequest<Result<List<ResBrandDto>>>;

    public class GetAllBrandsHandler : IRequestHandler<GetAllBrandsQuery, Result<List<ResBrandDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        
        public GetAllBrandsHandler(IDistributedCache cache, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<Result<List<ResBrandDto>>> Handle(GetAllBrandsQuery query, CancellationToken ct)
        {
            try
            {
                string cacheKey = $"brand";
                string cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var cachedList = JsonSerializer.Deserialize<List<ResBrandDto>>(cachedData);
                    if (cachedList != null)
                    {
                        return Result<List<ResBrandDto>>.Success(cachedList);
                    }
                }
                
                var result = await _unitOfWork.BrandRepository.GetAllBrandsAsync(query.Take, ct);
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(2));

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheOptions, ct);
                return Result<List<ResBrandDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<ResBrandDto>>.Failure(ex.Message);
            }
        }
    }
}
