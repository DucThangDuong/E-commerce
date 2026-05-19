namespace Application.DTOs.Response
{
    public class ResOrder
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public List<ResOrderWithItems> OrderItems { get; set; } = new List<ResOrderWithItems>();
    }
    public class ResOrderWithItems
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; } = null!;
        public int quantity { get; set; }
        public decimal unitPriceAtPurchase { get; set; }
        public string name { get; set; } = null!;
        public decimal basePrice { get; set; }
        public List<string> imageUrl { get; set; } = new List<string>();
    }
}
