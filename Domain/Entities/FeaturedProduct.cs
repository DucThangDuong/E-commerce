using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class FeaturedProduct
{
    public int FeaturedId { get; set; }

    public int ProductId { get; set; }

    public int? DisplayOrder { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
