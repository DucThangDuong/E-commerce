using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class OrderItem
{
    public int OrderId { get; set; }

    public int ColorId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPriceAtPurchase { get; set; }

    public virtual ProductColor Color { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
