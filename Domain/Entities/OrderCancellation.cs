using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class OrderCancellation
{
    public int CancellationId { get; set; }

    public int OrderId { get; set; }

    public int ReasonId { get; set; }

    public string? CustomReasonText { get; set; }

    public DateTime CanceledAt { get; set; }

    public int CanceledByUserId { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual CancellationReason Reason { get; set; } = null!;
}
