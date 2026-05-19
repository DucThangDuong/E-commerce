using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CouponUsage
{
    public int UsageId { get; set; }

    public int CouponId { get; set; }

    public int CustomerId { get; set; }

    public int OrderId { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Coupon Coupon { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
