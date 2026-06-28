using Application.Common;
using Application.Interfaces;
using Application.IServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Products.Queries
{
    public class ResPromotionWithProductsDto
    {
        public int PromotionId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DiscountType { get; set; } = null!;
        public decimal DiscountValue { get; set; }
        public List<ResPromotionProductItemDto> Products { get; set; } = new List<ResPromotionProductItemDto>();
    }

    public class ResPromotionProductItemDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int TotalSlots { get; set; }
        public int RemainingSlots { get; set; }
    }

    public record GetActivePromotionsQuery() : IRequest<Result<List<ResPromotionWithProductsDto>>>;

    public class GetActivePromotionsHandler : IRequestHandler<GetActivePromotionsQuery, Result<List<ResPromotionWithProductsDto>>>
    {
        private readonly IAppReadDbContext _db;
        private readonly ICacheService _cache;

        public GetActivePromotionsHandler(IAppReadDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Result<List<ResPromotionWithProductsDto>>> Handle(GetActivePromotionsQuery request, CancellationToken ct)
        {
            try
            {
                var now = DateTime.UtcNow;
                string cacheKey = "active_promotions";

                var promotions = await _cache.GetOrSetAsync(cacheKey, async () => 
                {
                    var promotionsDb = await _db.Promotions
                        .AsNoTracking()
                        .Include(p => p.Products)
                            .ThenInclude(p => p.ProductImages)
                        .Include(p => p.Products)
                            .ThenInclude(p => p.ProductColors)
                                .ThenInclude(pc => pc.Vehicles)
                        .Where(p => p.IsActive == true && p.StartDate <= now && p.EndDate >= now)
                        .OrderBy(p => p.EndDate)
                        .ToListAsync(ct);

                    var rng = new Random();

                    return promotionsDb.Select(p => new ResPromotionWithProductsDto
                    {
                        PromotionId = p.PromotionId,
                        Name = p.Name,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        DiscountType = p.DiscountType,
                        DiscountValue = p.DiscountValue,
                        Products = p.Products.OrderBy(x => rng.Next()).Select(prod => new ResPromotionProductItemDto
                        {
                            ProductId = prod.ProductId,
                            Name = prod.Name,
                            Description = prod.Description,
                            ImageUrl = prod.ProductImages.Where(pi => pi.DisplayOrder == 1).OrderBy(x => rng.Next()).Select(pi => pi.ImageUrl).FirstOrDefault() 
                                       ?? prod.ProductImages.OrderBy(x => rng.Next()).Select(pi => pi.ImageUrl).FirstOrDefault(),
                            BasePrice = prod.BasePrice,
                            DiscountedPrice = p.DiscountType.ToLower().Contains("percent")
                                ? prod.BasePrice - (prod.BasePrice * p.DiscountValue / 100M)
                                : prod.BasePrice - p.DiscountValue,
                            TotalSlots = prod.ProductColors.SelectMany(c => c.Vehicles).Count(),
                            RemainingSlots = prod.ProductColors.SelectMany(c => c.Vehicles).Count(v => v.Status == "Available")
                        }).ToList()
                    }).ToList();
                }, TimeSpan.FromMinutes(15));

                return Result<List<ResPromotionWithProductsDto>>.Success(promotions ?? new List<ResPromotionWithProductsDto>());
            }
            catch (Exception ex)
            {
                return Result<List<ResPromotionWithProductsDto>>.Failure($"Lỗi khi lấy danh sách khuyến mãi: {ex.Message}", 500);
            }
        }
    }
}
