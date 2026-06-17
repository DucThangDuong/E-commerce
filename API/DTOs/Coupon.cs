using System.Collections.Generic;

namespace API.DTOs
{
    public class ReqCheckCoupon
    {
        public string CouponCode { get; set; } = null!;
        public List<ProductOrder> Items { get; set; } = null!;
    }
}
