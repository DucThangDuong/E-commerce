using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CancellationReason
{
    public int ReasonId { get; set; }

    public string Code { get; set; } = null!;

    public string Content { get; set; } = null!;

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public virtual ICollection<OrderCancellation> OrderCancellations { get; set; } = new List<OrderCancellation>();
}
