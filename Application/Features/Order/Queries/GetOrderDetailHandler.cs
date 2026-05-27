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
                        OriginalAmount = e.OrderItems.Sum(oi => oi.Quantity * oi.UnitPriceAtPurchase),
                        DiscountAmount = e.DiscountAmount,
                        Status = e.Status,
                        PaymentStatus = e.Payment != null ? e.Payment.PaymentStatus : "",
                        TotalItems= e.OrderItems.Sum(oi => oi.Quantity),
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
                    .FirstOrDefaultAsync(cancellationToken);

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
