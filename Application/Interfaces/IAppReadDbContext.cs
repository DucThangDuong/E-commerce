using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces
{
    public interface IAppReadDbContext
    {
        DbSet<Product> Products { get; }
        DbSet<Brand> Brands { get; }
        DbSet<Category> Categories { get; }
        DbSet<Cart> Carts { get; }
        DbSet<Customer> Customers { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderItem> OrderItems { get; }
        DbSet<Inventory> Inventories { get; }
        DbSet<Payment> Payments { get; }
        DbSet<ProductImage> ProductImages { get; }
        DbSet<FeaturedProduct> FeaturedProducts { get; }
    }
}
