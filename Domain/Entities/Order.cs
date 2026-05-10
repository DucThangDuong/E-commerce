using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal TotalAmount { get; set; }

    public int Status { get; set; }

    public int PaymentStatus { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderShippingDetail? OrderShippingDetail { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
