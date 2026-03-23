using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ReqCreateProductDto
    {
        public int category_id { get; set; }
        public string name { get; set; } = null!;
        public string? description { get; set; }
        public decimal base_price { get; set; }
        public int stock_quantity { get; set; }
        public int brand_id { get; set; }
        public List<IFormFile>? images { get; set; }
    }
}
