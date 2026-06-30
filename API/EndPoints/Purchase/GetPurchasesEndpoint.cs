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
                    .AsSplitQuery()
                    .Where(e => e.CustomerId == customerId);

                if (!string.IsNullOrEmpty(req.Status))
                {
                    string statusLower = req.Status.ToLower();
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
                        OriginalAmount = e.OrderItems.Sum(oi => oi.UnitPriceAtPurchase),
                        DiscountAmount = e.DiscountAmount,
                        Status = e.Status,
                        PaymentStatus = e.Payment != null ? e.Payment.PaymentStatus : "",
                        OrderItems = e.OrderItems.Select(oi => new ResOrderWithItems
                        {
                            name = oi.Vehicle.Color.Product.Name,
                            ColorId = oi.Vehicle.ColorId,
                            ColorName = oi.Vehicle.Color.ColorName,
                            quantity = 1,
                            unitPriceAtPurchase = oi.UnitPriceAtPurchase,
                            basePrice = oi.Vehicle.Color.Product.BasePrice,
                            imageUrl = oi.Vehicle.Color.Product.ProductImages.Where(pi => pi.ColorId == null || pi.ColorId == oi.Vehicle.ColorId).Select(pi => pi.ImageUrl).ToList()
                        }).ToList()
                    })
                    .OrderByDescending(e => e.OrderDate)
                    .ToListAsync(ct);

                foreach (var order in orders)
                {
                    order.OrderItems = order.OrderItems
                        .GroupBy(oi => new { oi.ColorId, oi.ColorName, oi.name, oi.unitPriceAtPurchase, oi.basePrice })
                        .Select(g => new ResOrderWithItems
                        {
                            ColorId = g.Key.ColorId,
                            ColorName = g.Key.ColorName,
                            name = g.Key.name,
                            unitPriceAtPurchase = g.Key.unitPriceAtPurchase,
                            basePrice = g.Key.basePrice,
                            quantity = g.Sum(x => x.quantity),
                            imageUrl = g.First().imageUrl
                        }).ToList();
                }

                await this.SendApiResponseAsync(Result<List<ResOrder>>.Success(orders), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Result<List<ResOrder>>.Failure("An internal error occurred while fetching orders.", 500), ct);
            }
        }
    }
}
