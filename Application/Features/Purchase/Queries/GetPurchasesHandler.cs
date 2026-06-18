using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;
namespace Application.Features.Purchase.Queries
{
    public record GetPurchasesQuery(int CustomerId, string? Status) : IRequest<Result<List<ResOrder>>>;

    public class GetPurchasesHandler : IRequestHandler<GetPurchasesQuery, Result<List<ResOrder>>>
    {
        private readonly IAppReadDbContext _db;

        public GetPurchasesHandler(IAppReadDbContext db)
        {
            _db = db;
        }

        public async Task<Result<List<ResOrder>>> Handle(GetPurchasesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _db.Orders
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(e => e.CustomerId == request.CustomerId);

                if (!string.IsNullOrEmpty(request.Status))
                {
                    string statusLower = request.Status.ToLower();
                    if (statusLower == OrderStatus.Pending.ToString().ToLower())
                    {
                        query = query.Where(o => o.Status == OrderStatus.Processing_Payment.ToString() || o.Status == OrderStatus.Shipping.ToString()
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

                var orders = await query
                    .Select(e => new ResOrder
                    {
                        Address = e.OrderShippingDetail != null ? e.OrderShippingDetail.StreetAddress : "",
                        PhoneNumber = e.OrderShippingDetail != null ? e.OrderShippingDetail.RecipientPhone : null,
                        OrderId = e.OrderId,
                        OrderDate = e.OrderDate,
                        UpdatedAt = e.UpdatedAt,
                        TotalAmount = e.TotalAmount,
                        OriginalAmount = e.OrderItems.Sum(oi => oi.Quantity * oi.UnitPriceAtPurchase),
                        DiscountAmount = e.DiscountAmount,
                        Status = e.Status,
                        PaymentStatus = e.Payment != null ? e.Payment.PaymentStatus : "",
                        OrderItems = e.OrderItems.Select(oi => new ResOrderWithItems
                        {
                            name = oi.Color.Product.Name,
                            ColorId = oi.ColorId,
                            ColorName = oi.Color.ColorName,
                            quantity = oi.Quantity,
                            unitPriceAtPurchase = oi.UnitPriceAtPurchase,
                            basePrice = oi.Color.Product.BasePrice,
                            imageUrl = oi.Color.Product.ProductImages.Where(pi => pi.ColorId == null || pi.ColorId == oi.ColorId).Select(pi => pi.ImageUrl).ToList()
                        }).ToList()
                    })
                    .OrderByDescending(e => e.OrderDate)
                    .ToListAsync(cancellationToken);

                return Result<List<ResOrder>>.Success(orders);
            }
            catch (Exception ex)
            {
                return Result<List<ResOrder>>.Failure("An internal error occurred while fetching orders.", 500);
            }
        }
    }
}
