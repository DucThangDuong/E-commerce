using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Order.Queries
{
    public record GetOrderOfCustomerQuery(int CustomerId) : IRequest<Result<List<ResOrder>>>;
    public class GetOrderOfCustomerHandler : IRequestHandler<GetOrderOfCustomerQuery, Result<List<ResOrder>>>
    {
        private readonly IAppReadDbContext _db;

        public GetOrderOfCustomerHandler(IAppReadDbContext db)
        {
            _db = db;
        }

        public async Task<Result<List<ResOrder>>> Handle(GetOrderOfCustomerQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var orders = await _db.Orders
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(e => e.CustomerId == request.CustomerId)
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
                    .ToListAsync(cancellationToken);

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
                return Result<List<ResOrder>>.Success(orders);
            }
            catch (Exception ex)
            {
                return Result<List<ResOrder>>.Failure("An internal error occurred while fetching orders.", 500);
            }
        }
    }
}
