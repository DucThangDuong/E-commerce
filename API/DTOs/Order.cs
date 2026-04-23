namespace API.DTOs
{
    public class ReqAddNewOrder
    {
        public List<ProductOrder> Items { get; set; } = null!;
    }
    public class ProductOrder
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class ReqOrderInfo
    {
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int TypePayment { get; set; } // 1: Thanh toán online, 0: Thanh toán khi nhận hàng
    }
}
