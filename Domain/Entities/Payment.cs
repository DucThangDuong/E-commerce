using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string Provider { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
