using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.Queries
{
    public record GetItemCartCustomerQuery(int customerId);
    public class GetItemCartCustomerHandler : IQueryHandler<GetItemCartCustomerQuery, List<ResCartDto>>
    {
        private readonly IUnitOfWork _context;
        public GetItemCartCustomerHandler(IUnitOfWork context)
        {
            _context = context;
        }
        public async Task<Result<List<ResCartDto>>> HandleAsync(GetItemCartCustomerQuery query, CancellationToken ct = default)
        {
            try
            {

                var result = _context.Context.Carts.AsNoTracking()
                    .Where(e => e.CustomerId == query.customerId)
                    .Include(c => c.Product)
                    .ThenInclude(c => c.Category)
                    .Include(e => e.Product.Inventory)
                    .Include(e => e.Product.ProductImages)
                    .Select(e => new ResCartDto
                    {
                        BasePrice = e.Product.BasePrice,
                        CartId = e.CartId,
                        CategoryId = e.Product.CategoryId,
                        Description = e.Product.Description,
                        Name = e.Product.Name,
                        ProductId = e.Product.ProductId,
                        Quantity = e.Quantity,
                        StockQuantity = e.Product.Inventory.StockQuantity,
                        imageUrl = e.Product.ProductImages.Select(e => e.ImageUrl).ToList(),
                    }).ToList();

                return Result<List<ResCartDto>>.Success(result);

            }
            catch (Exception ex)
            {
                return Result<List<ResCartDto>>.Failure(ex.Message);
            }
        }
    }
}
