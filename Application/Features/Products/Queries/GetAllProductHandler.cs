using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries
{
    public record GetAllProductQuery(int Skip, int Take) : IRequest<Result<ResPagedProductDto>>;

    public class GetAllProductHandler : IRequestHandler<GetAllProductQuery, Result<ResPagedProductDto>>
    {
        private readonly IAppReadDbContext _db;

        public GetAllProductHandler(IAppReadDbContext db)
        {
            _db = db;
        }

        public async Task<Result<ResPagedProductDto>> Handle(GetAllProductQuery query, CancellationToken ct)
        {
            try
            {
                int totalItems = await _db.Products.CountAsync(ct);

                var products = await _db.Products
                    .AsNoTracking()
                    .OrderBy(e => e.ProductId)
                    .Skip(query.Skip)
                    .Take(query.Take > 0 ? query.Take : 10)
                    .Select(e => new ResProductDto
                    {
                        BasePrice = e.BasePrice,
                        CategoryId = e.CategoryId,
                        BrandId = e.BrandId,
                        Description = e.Description,
                        Name = e.Name,
                        ProductId = e.ProductId,
                        ImageUrls = e.ProductImages.Where(pi => pi.ColorId == null).Select(pi => pi.ImageUrl).ToList(),
                        Colors = e.ProductColors.Select(pc => new ResProductColorDto
                        {
                            ColorId = pc.ColorId,
                            ColorName = pc.ColorName,
                            PriceAdjustment = pc.PriceAdjustment,
                            StockQuantity = pc.Vehicles.Count(v => v.Status == "Available"),
                            ImageUrls = pc.ProductImages.Select(pi => pi.ImageUrl).ToList()
                        }).ToList()
                    })
                    .ToListAsync(ct);

                int take = query.Take > 0 ? query.Take : 10;
                var pagedResult = new ResPagedProductDto
                {
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)take),
                    CurrentPage = (query.Skip / take) + 1,
                    PageSize = take,
                    Products = products
                };

                return Result<ResPagedProductDto>.Success(pagedResult, 200);
            }
            catch (Exception ex)
            {
                return Result<ResPagedProductDto>.Failure($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}", 500);
            }
        }
    }
}
