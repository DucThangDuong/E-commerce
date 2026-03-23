using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int ProductId { get; set; }

    public int StockQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Product Product { get; set; } = null!;
}
