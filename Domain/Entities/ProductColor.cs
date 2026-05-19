using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ProductColor
{
    public int ColorId { get; set; }

    public int ProductId { get; set; }

    public string ColorName { get; set; } = null!;

    public decimal? PriceAdjustment { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Inventory? Inventory { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
