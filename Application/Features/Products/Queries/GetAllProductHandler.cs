using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Products.Queries
{
    public record GetAllProductQuery(int skip,int take);

    public class GetAllProductHandler: IQueryHandler<GetAllProductQuery,List<ResProductDto>>
    {
        private readonly IUnitOfWork _context;
        public GetAllProductHandler(IUnitOfWork context)
        {
            _context = context;
        }
        public async Task<Result<List<ResProductDto>>> HandleAsync(GetAllProductQuery query, CancellationToken ct = default)
        {
            try
            {
                var products = await _context.Context.Products
                    .AsNoTracking()
                    .OrderBy(e => e.ProductId)
                    .Skip(query.skip)
                    .Take(query.take)
                    .Include(e => e.Inventory)
                    .Include(e => e.ProductImages)
                    .Select(e => new ResProductDto
                    {
                        BasePrice = e.BasePrice,
                        CategoryId = e.CategoryId,
                        Description = e.Description,
                        Name = e.Name,
                        ProductId = e.ProductId,
                        StockQuantity = e.Inventory.StockQuantity,
                        imageUrl = e.ProductImages.Select(e => e.ImageUrl).ToList(),
                    })
                    .ToListAsync(ct);

                return Result<List<ResProductDto>>.Success(products);
            }
            catch (Exception ex) {
                return Result<List<ResProductDto>>.Failure($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}", 500);
            }
        }
    }
}
