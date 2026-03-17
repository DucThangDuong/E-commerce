using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Response
{
    public class ResCartDto
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }
        public int CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public List<string>? imageUrl { get; set; }
    }
}
