using System;
using System.Collections.Generic;
using Domain.Common;
using Domain.Events;

namespace Domain.Entities;

public partial class Customer : BaseEntity
{
    public int CustomerId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Role { get; set; }

    public string? LoginProvider { get; set; }

    public string? GoogleId { get; set; }

    public string? CustomAvatar { get; set; }

    public string? GoogleAvatar { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<WarrantyBook> WarrantyBooks { get; set; } = new List<WarrantyBook>();

    public static Customer Create(string name, string email, string passwordHash)
    {
        var customer = new Customer
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            Role = "User",
            IsActive = true,
            LoginProvider = "Custom"
        };
        
        customer.AddDomainEvent(new UserCreatedEvent(name, email));
        
        return customer;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        CustomAvatar = "default-avatar.jpg";
    }
}
