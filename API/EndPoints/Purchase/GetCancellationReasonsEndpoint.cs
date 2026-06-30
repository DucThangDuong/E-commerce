using API.Extensions;
using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace API.EndPoints.Purchase
{
    public class GetCancellationReasonsEndpoint : EndpointWithoutRequest
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetCancellationReasonsEndpoint(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public override void Configure()
        {
            Get("/cancellation-reasons");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var cacheKey = "cancellation_reasons_all";
                var cached = await _cache.GetAsync<List<ResCancellationReasonDto>>(cacheKey);
                if (cached != null) 
                {
                    await this.SendApiResponseAsync(Result<List<ResCancellationReasonDto>>.Success(cached), ct);
                    return;
                }

                var reasons = await _db.CancellationReasons
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new ResCancellationReasonDto
                    {
                        ReasonId = x.ReasonId,
                        Code = x.Code,
                        Content = x.Content,
                        DisplayOrder = x.DisplayOrder
                    })
                    .ToListAsync(ct);

                await _cache.SetAsync(cacheKey, reasons, TimeSpan.FromHours(24));

                await this.SendApiResponseAsync(Result<List<ResCancellationReasonDto>>.Success(reasons), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Result<List<ResCancellationReasonDto>>.Failure($"Lỗi khi lấy danh sách lý do hủy: {ex.Message}", 500), ct);
            }
        }
    }
}
