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
    public class ReqOrderInfo
    {
        public string ReservationId { get; set; } = null!;
        public double Amount { get; set; }
        public string OrderDescription { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int TypePayment { get; set; } // 1: Thanh toán online, 0: Thanh toán khi nhận hàng
    }
}
