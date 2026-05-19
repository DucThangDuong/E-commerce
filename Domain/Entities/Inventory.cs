using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int ColorId { get; set; }

    public int StockQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual ProductColor Color { get; set; } = null!;
}
