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
                        BasePrice = e.Color.Product.BasePrice,
                        CartId = e.CartId,
                        CategoryId = e.Color.Product.CategoryId,
                        Description = e.Color.Product.Description,
                        Name = e.Color.Product.Name,
                        ProductId = e.Color.ProductId,
                        ColorId = e.ColorId,
                        ColorName = e.Color.ColorName,
                        Quantity = e.Quantity,
                        StockQuantity = e.Color.Inventory != null ? e.Color.Inventory.StockQuantity : 0,
                        imageUrl = e.Color.Product.ProductImages.Where(pi => pi.ColorId == null || pi.ColorId == e.ColorId).Select(pi => pi.ImageUrl).ToList(),
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
