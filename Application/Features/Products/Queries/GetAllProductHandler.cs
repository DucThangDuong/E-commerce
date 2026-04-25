using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries
{
    public record GetAllProductQuery(int Skip, int Take) : IRequest<Result<List<ResProductDto>>>;

    public class GetAllProductHandler : IRequestHandler<GetAllProductQuery, Result<List<ResProductDto>>>
    {
        private readonly IAppReadDbContext _db;

        public GetAllProductHandler(IAppReadDbContext db)
        {
            _db = db;
        }

        public async Task<Result<List<ResProductDto>>> Handle(GetAllProductQuery query, CancellationToken ct)
        {
            try
            {
                var products = await _db.Products
                    .AsNoTracking()
                    .OrderBy(e => e.ProductId)
                    .Skip(query.Skip)
                    .Take(query.Take)
                    .Select(e => new ResProductDto
                    {
                        BasePrice = e.BasePrice,
                        CategoryId = e.CategoryId,
                        BrandId = e.BrandId,
                        Description = e.Description,
                        Name = e.Name,
                        ProductId = e.ProductId,
                        StockQuantity = e.Inventory != null ? e.Inventory.StockQuantity : 0,
                        imageUrl = e.ProductImages.Select(pi => pi.ImageUrl).ToList(),
                    })
                    .ToListAsync(ct);
                return Result<List<ResProductDto>>.Success(products,200);
            }
            catch (Exception ex)
            {
                return Result<List<ResProductDto>>.Failure($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}", 500);
            }
        }
    }
}
