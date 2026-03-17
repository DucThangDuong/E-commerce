using Application.Interfaces;
using Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
namespace Infrastructure;

public partial class EcommerceOrderSystemContext : DbContext, IApplicationDbContext
{
    public EcommerceOrderSystemContext()
    {
    }

    public EcommerceOrderSystemContext(DbContextOptions<EcommerceOrderSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }


    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Cart__2EF52A27A10CD1AC");

            entity.ToTable("Cart");

            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Customer).WithMany(p => p.Carts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__customer_i__6E01572D");

            entity.HasOne(d => d.Product).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__product_id__6EF57B66");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__D54EE9B4686DDD42");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Picture)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("picture");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__CD65CB85BAC1864C");

            entity.HasIndex(e => e.Email, "UQ__Customer__AB6E616413905FB2").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.CustomAvatar)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValue("default-avatar.jpg")
                .HasColumnName("customAvatar");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.GoogleAvatar)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("googleAvatar");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("googleId");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.LoginProvider)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("loginProvider");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("passwordHash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasColumnName("refreshToken");
            entity.Property(e => e.RefreshTokenExpiryTime)
                .HasColumnType("datetime")
                .HasColumnName("refreshTokenExpiryTime");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("User")
                .HasColumnName("role");
        });


        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__Inventor__B59ACC491A29B1CA");

            entity.ToTable("Inventory");

            entity.HasIndex(e => e.ProductId, "UQ__Inventor__47027DF401DB4DFE").IsUnique();

            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_updated");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ReservedQuantity).HasColumnName("reserved_quantity");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");

            entity.HasOne(d => d.Product).WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__produ__5BE2A6F2");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__465962296FF8C675");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_amount");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__customer__60A75C0F");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductId }).HasName("PK__OrderIte__022945F6DE0B37EE");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPriceAtPurchase)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("unit_price_at_purchase");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__order__6477ECF3");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__produ__656C112C");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__ED1FC9EACF97B0E6");

            entity.HasIndex(e => e.OrderId, "UQ__Payments__465962289B9DA9A0").IsUnique();

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Unpaid")
                .HasColumnName("payment_status");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("provider");

            entity.HasOne(d => d.Order).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__order___6A30C649");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__47027DF516B9ACBB");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.BasePrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("base_price");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__catego__5535A963");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ProductI__DC9AC955AADBD720");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("image_url");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductIm__produ__74AE54BC");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
