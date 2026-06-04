namespace API.DTOs
{
    public class ReqAddNewOrder
    {
        public List<ProductOrder> Items { get; set; } = null!;
    }
    public class ProductOrder
    {
        public int ColorId { get; set; }
        public int Quantity { get; set; }
    }
    public class ReqCalculateOrder
    {
        public List<ProductOrder> Items { get; set; } = null!;
        public string? CouponCode { get; set; }
    }
    public class ReqCreatePayment
    {
        public List<ProductOrder> Items { get; set; } = null!;
        public string? CouponCode { get; set; }
        public string FullName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int TypePayment { get; set; } // 1: Thanh toán online, 0: Thanh toán khi nhận hàng
    }
}
