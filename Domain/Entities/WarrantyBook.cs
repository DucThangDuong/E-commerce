using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class WarrantyBook
{
    public int WarrantyId { get; set; }

    public int VehicleId { get; set; }

    public int CustomerId { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime ValidUntil { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;
}
