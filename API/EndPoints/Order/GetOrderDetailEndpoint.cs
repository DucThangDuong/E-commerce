using API.Extensions;
using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Order
{
    public class ReqGetOrderDetailDto
    {
        public int OrderId { get; set; }
    }

    public class GetOrderDetailEndpoint : Endpoint<ReqGetOrderDetailDto>
    {
        private readonly IAppReadDbContext _db;

        public GetOrderDetailEndpoint(IAppReadDbContext db)
        {
            _db = db;
        }

        public override void Configure()
        {
            Get("/order/{orderId}");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqGetOrderDetailDto req, CancellationToken ct)
        {
            try
            {
                var customerId = HttpContext.User.GetUserId();
                
                var order = await _db.Orders
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(e => e.CustomerId == customerId && e.OrderId == req.OrderId)
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
                        TotalItems = e.OrderItems.Count,
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
                    .FirstOrDefaultAsync(ct);

                if (order != null)
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

                if (order == null)
                {
                    await this.SendApiResponseAsync(Result<ResOrder>.Failure("Order not found or you don't have access to it.", 404), ct);
                    return;
                }

                await this.SendApiResponseAsync(Result<ResOrder>.Success(order), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Result<ResOrder>.Failure("An internal error occurred while fetching the order detail.", 500), ct);
            }
        }
    }
}
