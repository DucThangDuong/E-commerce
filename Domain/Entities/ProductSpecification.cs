using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ProductSpecification
{
    public int ProductId { get; set; }

    public int SpecId { get; set; }

    public string SpecValue { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Specification Spec { get; set; } = null!;
}
