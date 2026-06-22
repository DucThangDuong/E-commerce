using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Response
{
    public class ResProductColorDto
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; } = null!;
        public decimal? PriceAdjustment { get; set; }
        public int StockQuantity { get; set; }
        public List<string>? ImageUrls { get; set; }
    }

    public class ResProductSpecificationDto
    {
        public string SpecName { get; set; } = null!;
        public string SpecValue { get; set; } = null!;
    }

    public class ResProductDto
    {
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal BasePrice { get; set; }
        public List<string>? ImageUrls { get; set; } 
        public List<ResProductColorDto> Colors { get; set; } = new List<ResProductColorDto>();
        public List<ResProductSpecificationDto>? Specifications { get; set; }
    }
    public class ResPagedProductDto
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<ResProductDto> Products { get; set; } = new();
    }
    public class FileUploadDto
    {
        public Stream Stream { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }

    public class ResFeaturedProductDto
    {
        public int FeaturedId { get; set; }
        public int ProductId { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ResProductDto Product { get; set; } = null!;
    }

    public class ResSimpleFeaturedProductDto
    {
        public int ProductId { get; set; }
        public int? DisplayOrder { get; set; }
        public string? FirstColorImageUrl { get; set; }
    }
}
