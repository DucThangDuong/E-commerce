using API.Extensions;
using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Purchase
{
    public class ReqGetPurchaseDto
    {
        [QueryParam]
        public string? Status { get; set; }
        [QueryParam]
        public int PageIndex { get; set; } = 1;
        [QueryParam]
        public int PageSize { get; set; } = 10;
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class GetPurchasesEndpoint : Endpoint<ReqGetPurchaseDto>
    {
        private readonly IAppReadDbContext _db;

        public GetPurchasesEndpoint(IAppReadDbContext db)
        {
            _db = db;
        }

        public override void Configure()
        {
            Get("/purchase");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqGetPurchaseDto req, CancellationToken ct)
        {
            try
            {
                int customerId = HttpContext.User.GetUserId();
                
                var query = _db.Orders
                    .AsNoTracking()
                    .Where(e => e.CustomerId == customerId);

                if (!string.IsNullOrEmpty(req.Status))
                {
                    string statusLower = req.Status.ToLower();
                    if (statusLower == OrderStatus.Pending.ToString().ToLower())
                    {
                        query = query.Where(o => o.Status == OrderStatus.Shipping.ToString()
                        || o.Status == OrderStatus.Pending.ToString() || o.Status == OrderStatus.Confirmed.ToString());
                    }
                    else if (statusLower == OrderStatus.Completed.ToString().ToLower())
                    {
                        query = query.Where(o => o.Status == OrderStatus.Completed.ToString());
                    }
                    else if (statusLower == "cancelled")
                    {
                        query = query.Where(o => o.Status == OrderStatus.Cancelled.ToString() || o.Status == OrderStatus.Failed.ToString());
                    }
                }

                int totalRecords = await query.CountAsync(ct);
                int totalPages = (int)Math.Ceiling(totalRecords / (double)req.PageSize);

                var orders = await query
                    .OrderByDescending(e => e.UpdatedAt ?? e.OrderDate)
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .Select(e => new ResOrderSummary
                    {
                        OrderId = e.OrderId,
                        OrderDate = e.OrderDate,
                        TotalAmount = e.TotalAmount,
                        Status = e.Status
                    })
                    .ToListAsync(ct);

                var pagedResponse = new PagedResponse<ResOrderSummary>
                {
                    Items = orders,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    PageIndex = req.PageIndex,
                    PageSize = req.PageSize
                };

                await this.SendApiResponseAsync(Result<PagedResponse<ResOrderSummary>>.Success(pagedResponse), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Result<PagedResponse<ResOrderSummary>>.Failure("An internal error occurred while fetching orders.", 500), ct);
            }
        }
    }
}
