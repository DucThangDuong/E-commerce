using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int VehicleId { get; set; }

    public decimal UnitPriceAtPurchase { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;
}
