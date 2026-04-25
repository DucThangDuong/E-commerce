using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carts.Queries
{
    public record GetItemCartCustomerQuery(int CustomerId) : IRequest<Result<List<ResCartDto>>>;

    public class GetItemCartCustomerHandler : IRequestHandler<GetItemCartCustomerQuery, Result<List<ResCartDto>>>
    {
        private readonly IAppReadDbContext _db;

        public GetItemCartCustomerHandler(IAppReadDbContext db)
        {
            _db = db;
        }

        public async Task<Result<List<ResCartDto>>> Handle(GetItemCartCustomerQuery query, CancellationToken ct)
        {
            try
            {
                var result = await _db.Carts
                    .AsNoTracking()
                    .Where(e => e.CustomerId == query.CustomerId)
                    .Select(e => new ResCartDto
                    {
                        BasePrice = e.Product.BasePrice,
                        CartId = e.CartId,
                        CategoryId = e.Product.CategoryId,
                        Description = e.Product.Description,
                        Name = e.Product.Name,
                        ProductId = e.Product.ProductId,
                        Quantity = e.Quantity,
                        StockQuantity = e.Product.Inventory != null ? e.Product.Inventory.StockQuantity : 0,
                        imageUrl = e.Product.ProductImages.Select(pi => pi.ImageUrl).ToList(),
                    })
                    .ToListAsync(ct);
                return Result<List<ResCartDto>>.Success(result,200);
            }
            catch (Exception ex)
            {
                return Result<List<ResCartDto>>.Failure(ex.Message, 400);
            }
        }
    }
}
