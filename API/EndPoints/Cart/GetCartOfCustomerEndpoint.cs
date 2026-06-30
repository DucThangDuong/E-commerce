using API.Extensions;
using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Cart
{
    public class GetCartOfCustomerEndpoint : EndpointWithoutRequest
    {
        private readonly IAppReadDbContext _db;

        public GetCartOfCustomerEndpoint(IAppReadDbContext db)
        {
            _db = db;
        }

        public override void Configure()
        {
            Get("cart");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Options(x => x.RequireRateLimiting("cart_strict"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var result = await _db.Carts
                    .AsNoTracking()
                    .Where(e => e.CustomerId == userId)
                    .Select(e => new ResCartDto
                    {
                        BasePrice = e.Color.Product.BasePrice + (e.Color.PriceAdjustment ?? 0),
                        DiscountedPrice = (e.Color.Product.BasePrice - (e.Color.Product.Promotions
                            .Where(p => p.IsActive == true && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                            .Select(p => p.DiscountType.ToLower().Contains("percent") 
                                ? (e.Color.Product.BasePrice * p.DiscountValue / 100M) 
                                : p.DiscountValue)
                            .OrderByDescending(x => x)
                            .FirstOrDefault())) + (e.Color.PriceAdjustment ?? 0),
                        AppliedPromotion = e.Color.Product.Promotions
                            .Where(p => p.IsActive == true && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                            .Select(p => new ResProductPromotionDto
                            {
                                PromotionName = p.Name,
                                DiscountType = p.DiscountType,
                                DiscountValue = p.DiscountValue,
                                AmountReduced = p.DiscountType.ToLower().Contains("percent") 
                                    ? (e.Color.Product.BasePrice * p.DiscountValue / 100M) 
                                    : p.DiscountValue
                            })
                            .OrderByDescending(p => p.AmountReduced)
                            .FirstOrDefault(),
                        CartId = e.CartId,
                        CategoryId = e.Color.Product.CategoryId,
                        Description = e.Color.Product.Description,
                        Name = e.Color.Product.Name,
                        ProductId = e.Color.ProductId,
                        ColorId = e.ColorId,
                        ColorName = e.Color.ColorName,
                        Quantity = e.Quantity,
                        StockQuantity = e.Color.Vehicles.Count(v => v.Status == "Available"),
                        imageUrl = e.Color.Product.ProductImages.Where(pi => pi.ColorId == null || pi.ColorId == e.ColorId).Select(pi => pi.ImageUrl).ToList(),
                    })
                    .ToListAsync(ct);
                await this.SendApiResponseAsync(Result<List<ResCartDto>>.Success(result, 200), ct);
            }
            catch (Exception ex)
            {
                await this.SendApiResponseAsync(Result<List<ResCartDto>>.Failure(ex.Message, 400), ct);
            }
        }
    }
}
