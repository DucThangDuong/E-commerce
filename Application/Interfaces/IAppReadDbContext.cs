using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces
{
    public interface IAppReadDbContext
    {
        DbSet<Brand> Brands { get; }
        DbSet<CancellationReason> CancellationReasons { get; }

        DbSet<Cart> Carts { get; }

        DbSet<Category> Categories { get; }

        DbSet<Coupon> Coupons { get; }

        DbSet<CouponUsage> CouponUsages { get; }

        DbSet<Customer> Customers { get; }

        DbSet<FeaturedProduct> FeaturedProducts { get; }
        DbSet<Order> Orders { get; }

        DbSet<OrderCancellation> OrderCancellations { get; }

        DbSet<OrderItem> OrderItems { get; }

        DbSet<OrderShippingDetail> OrderShippingDetails { get; }

        DbSet<Payment> Payments { get; }

        DbSet<Product> Products { get; }

        DbSet<ProductColor> ProductColors { get; }

        DbSet<ProductImage> ProductImages { get; }

        DbSet<ProductSpecification> ProductSpecifications { get; }

        DbSet<Promotion> Promotions { get; }

        DbSet<Specification> Specifications { get; }
        DbSet<Vehicle> Vehicles { get; }
        DbSet<WarrantyBook> WarrantyBooks { get; }
    }
}
