using API.Extensions;
using Application.DTOs.Response;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace API.EndPoints.Product
{
    public class ReqGetProductDto
    {
        [QueryParam]
        public int take { get; set; } = 10;
        [QueryParam]
        public int skip { get; set; } = 0;
    }

    public class GetAllProductEndpoint : Endpoint<ReqGetProductDto>
    {
        private readonly IAppReadDbContext _db;

        public GetAllProductEndpoint(IAppReadDbContext db)
        {
            _db = db;
        }

        public override void Configure()
        {
            Get("/product");
            AllowAnonymous();
            Options(x => x.RequireRateLimiting("search_strict"));
        }

        public override async Task HandleAsync(ReqGetProductDto req, CancellationToken ct)
        {
            try
            {
                int totalItems = await _db.Products.CountAsync(ct);

                var products = await _db.Products
                    .AsNoTracking()
                    .OrderBy(e => e.ProductId)
                    .Skip(req.skip)
                    .Take(req.take > 0 ? req.take : 10)
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

                int take = req.take > 0 ? req.take : 10;
                var pagedResult = new ResPagedProductDto
                {
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)take),
                    CurrentPage = (req.skip / take) + 1,
                    PageSize = take,
                    Products = products
                };

                await this.SendApiResponseAsync(Application.Common.Result<ResPagedProductDto>.Success(pagedResult, 200), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Application.Common.Result<ResPagedProductDto>.Failure($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}", 500), ct);
            }
        }
    }
}
