using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Order.Queries
{
    public record GetAllCancellationReasonsQuery() : IRequest<Result<List<ResCancellationReasonDto>>>;
    public class GetAllCancellationReasonsHandler : IRequestHandler<GetAllCancellationReasonsQuery, Result<List<ResCancellationReasonDto>>>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetAllCancellationReasonsHandler(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Result<List<ResCancellationReasonDto>>> Handle(GetAllCancellationReasonsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = "cancellation_reasons_all";
                var cached = await _cache.GetAsync<List<ResCancellationReasonDto>>(cacheKey);
                if (cached != null) return Result<List<ResCancellationReasonDto>>.Success(cached);

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
                    .ToListAsync(cancellationToken);

                await _cache.SetAsync(cacheKey, reasons, TimeSpan.FromHours(24));

                return Result<List<ResCancellationReasonDto>>.Success(reasons);
            }
            catch (Exception ex)
            {
                return Result<List<ResCancellationReasonDto>>.Failure($"Lỗi khi lấy danh sách lý do hủy: {ex.Message}", 500);
            }
        }
    }
}
