using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Vehicle
{
    public int VehicleId { get; set; }

    public int ColorId { get; set; }

    public string Vin { get; set; } = null!;

    public string EngineNumber { get; set; } = null!;

    public string? Status { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public DateTime? ImportedAt { get; set; }

    public virtual ProductColor Color { get; set; } = null!;

    public virtual OrderItem? OrderItem { get; set; }

    public virtual WarrantyBook? WarrantyBook { get; set; }
}
