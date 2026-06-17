using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Response
{
    public record ResCheckCoupon
    {
        public decimal SubTotal { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal FinalAmount { get; init; }
        public string CouponCode { get; init; } = null!;
        public string CouponName { get; init; } = null!;
        public string DiscountType { get; init; } = null!;
        public decimal DiscountValue { get; init; }
    }
}
