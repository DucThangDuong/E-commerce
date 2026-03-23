using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Response
{
    public class ResProductDto
    {
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public List<string>? imageUrl { get; set; } 
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
}
