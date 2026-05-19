using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Specification
{
    public int SpecId { get; set; }

    public string SpecName { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public virtual ICollection<ProductSpecification> ProductSpecifications { get; set; } = new List<ProductSpecification>();
}
