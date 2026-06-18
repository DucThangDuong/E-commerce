using Application.Features.Order.Commands;

namespace Application.DTOs.Response
{
    public class ResOrder
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public int TotalItems { get; set; }
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
    public record CartItemRequest(int ColorId, int Quantity);
    public record ValidateCartResponse
    {
        public decimal SubTotal { get; init; }
        public List<ValidatedCartItem> Items { get; init; } = new();
    }
    public record ValidatedCartItem
    {
        public int ColorId { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal LineTotal { get; init; }
        public int AvailableStock { get; init; }
    }
    public record CalculateOrderResponse
    {
        public decimal SubTotal { get; init; }
        public decimal ShippingFee { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal FinalAmount { get; init; }
        public int? CouponId { get; init; }
        public string? CouponCode { get; init; }
    }
}
