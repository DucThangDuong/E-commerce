using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string? Role { get; set; }

    public string? LoginProvider { get; set; }

    public string? GoogleId { get; set; }

    public string? CustomAvatar { get; set; }

    public string? GoogleAvatar { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
