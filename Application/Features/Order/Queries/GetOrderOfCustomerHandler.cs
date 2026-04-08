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
            var orders = await _db.Orders
                .AsNoTracking()
                .Where(e => e.CustomerId == request.CustomerId)
                .Select(e => new ResOrder
                {
                    Address = e.Payment != null ? e.Payment.Address : "",
                    PhoneNumber = e.Payment != null ? e.Payment.PhoneNumber : null,
                    OrderId = e.OrderId,
                    OrderDate = e.OrderDate,
                    TotalAmount = e.TotalAmount,
                    Status = e.Status,
                    PaymentStatus = e.Payment != null ? e.Payment.PaymentStatus : "Unpaid",
                    OrderItems = e.OrderItems.Select(oi => new ResOrderWithItems
                    {
                        name = oi.Product.Name,
                        quantity = oi.Quantity,
                        unitPriceAtPurchase = oi.UnitPriceAtPurchase,
                        basePrice = oi.Product.BasePrice,
                        imageUrl = oi.Product.ProductImages.Select(pi => pi.ImageUrl).ToList()
                    }).ToList()
                })
                .OrderByDescending(e => e.OrderDate)
                .ToListAsync(cancellationToken);
            return Result<List<ResOrder>>.Success(orders);
        }
    }
}
