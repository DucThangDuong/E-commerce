using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class OrderShippingDetail
{
    public int OrderId { get; set; }

    public string RecipientName { get; set; } = null!;

    public string RecipientPhone { get; set; } = null!;

    public string StreetAddress { get; set; } = null!;

    public string? CustomerNote { get; set; }

    public virtual Order Order { get; set; } = null!;
}
