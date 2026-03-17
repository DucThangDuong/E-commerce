using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ReqCreateCartDto
    {
        public int product_id { get; set; }
        public int quantity { get; set; }
    }
}
