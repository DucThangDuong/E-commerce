using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Queries
{
    public record GetOrderDetailQuery(int CustomerId, int OrderId) : IRequest<Result<ResOrder>>;

    public class GetOrderDetailHandler : IRequestHandler<GetOrderDetailQuery, Result<ResOrder>>
    {
        private readonly IAppReadDbContext _db;

        public GetOrderDetailHandler(IAppReadDbContext db)
        {
            _db = db;
        }

        public async Task<Result<ResOrder>> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _db.Orders
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(e => e.CustomerId == request.CustomerId && e.OrderId == request.OrderId)
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
                    .FirstOrDefaultAsync(cancellationToken);

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
                    return Result<ResOrder>.Failure("Order not found or you don't have access to it.", 404);
                }

                return Result<ResOrder>.Success(order);
            }
            catch (System.Exception)
            {
                return Result<ResOrder>.Failure("An internal error occurred while fetching the order detail.", 500);
            }
        }
    }
}
