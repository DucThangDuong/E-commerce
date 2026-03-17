using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Cart> Carts { get; set; }

        DbSet<Category> Categories { get; set; }

        DbSet<Customer> Customers { get; set; }

        DbSet<Inventory> Inventories { get; set; }

        DbSet<Order> Orders { get; set; }

        DbSet<OrderItem> OrderItems { get; set; }

        DbSet<Payment> Payments { get; set; }

        DbSet<Product> Products { get; set; }
        DbSet<ProductImage> ProductImages { get; set; }
    }
}
