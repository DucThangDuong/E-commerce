using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Application.Features.Products.Queries
{
    public record GetFilteredProductsQuery(List<int>? CategoryIds, List<int>? BrandIds, int Skip, int Take) : IRequest<Result<List<ResProductDto>>>;

    public class GetFilteredProductsHandler : IRequestHandler<GetFilteredProductsQuery, Result<List<ResProductDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public GetFilteredProductsHandler(IUnitOfWork unitOfWork, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<Result<List<ResProductDto>>> Handle(GetFilteredProductsQuery query, CancellationToken ct)
        {
            try
            {
                string cacheKey = $"products_{string.Join("_", query.CategoryIds ?? new List<int>())}" +
                    $"_{string.Join("_", query.BrandIds ?? new List<int>())}_{query.Skip}_{query.Take}";
                string cachedData = await _cache.GetStringAsync(cacheKey, ct);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var cachedProducts = JsonSerializer.Deserialize<List<ResProductDto>>(cachedData);
                    if (cachedProducts != null)
                    {
                        return Result<List<ResProductDto>>.Success(cachedProducts);
                    }
                }
                var result = await _unitOfWork.ProductRepository.GetFilteredProductsAsync(query.CategoryIds, query.BrandIds, query.Skip, query.Take, ct);
                if (result != null)
                {
                    var cacheOptions = new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheOptions, ct);
                }
                return Result<List<ResProductDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<ResProductDto>>.Failure(ex.Message);
            }
        }
    }
}
