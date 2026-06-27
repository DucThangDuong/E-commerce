using Application.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Domain.Entities;

namespace Infrastructure;

public partial class EcommerceContext : DbContext, IAppReadDbContext
{
    public EcommerceContext()
    {
    }

    public EcommerceContext(DbContextOptions<EcommerceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<CancellationReason> CancellationReasons { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<CouponUsage> CouponUsages { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<FeaturedProduct> FeaturedProducts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderCancellation> OrderCancellations { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderShippingDetail> OrderShippingDetails { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductColor> ProductColors { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductSpecification> ProductSpecifications { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Specification> Specifications { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<WarrantyBook> WarrantyBooks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brands__5E5A8E278C3B9938");

            entity.HasIndex(e => e.Name, "UQ__Brands__72E12F1B553F9648").IsUnique();

            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CancellationReason>(entity =>
        {
            entity.HasKey(e => e.ReasonId).HasName("PK__Cancella__846BB55436DA7FE2");

            entity.HasIndex(e => e.Code, "UQ__Cancella__357D4CF99EC5D9DF").IsUnique();

            entity.Property(e => e.ReasonId).HasColumnName("reason_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Content)
                .HasMaxLength(255)
                .HasColumnName("content");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Cart__2EF52A27A062300C");

            entity.ToTable("Cart");

            entity.HasIndex(e => new { e.CustomerId, e.ColorId }, "UQ__Cart__6C71F768104FE899").IsUnique();

            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.ColorId).HasColumnName("color_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Color).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__color_id__367C1819");

            entity.HasOne(d => d.Customer).WithMany(p => p.Carts)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Cart__customer_i__3587F3E0");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__D54EE9B46F895705");

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

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("PK__Coupons__58CF63897D7A696F");

            entity.HasIndex(e => e.Code, "UQ__Coupons__357D4CF9159CBAD8").IsUnique();

            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("discount_type");
            entity.Property(e => e.DiscountValue)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("discount_value");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxDiscountAmount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("max_discount_amount");
            entity.Property(e => e.MinOrderValue)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("min_order_value");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
            entity.Property(e => e.UsageLimitPerUser)
                .HasDefaultValue(1)
                .HasColumnName("usage_limit_per_user");
            entity.Property(e => e.UsedCount)
                .HasDefaultValue(0)
                .HasColumnName("used_count");
        });

        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.HasKey(e => e.UsageId).HasName("PK__CouponUs__B6B13A02C01BCB1D");

            entity.HasIndex(e => new { e.CouponId, e.CustomerId, e.OrderId }, "UQ__CouponUs__3C5F665236D3C957").IsUnique();

            entity.Property(e => e.UsageId).HasColumnName("usage_id");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UsedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("used_at");

            entity.HasOne(d => d.Coupon).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("FK__CouponUsa__coupo__245D67DE");

            entity.HasOne(d => d.Customer).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CouponUsa__custo__25518C17");

            entity.HasOne(d => d.Order).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CouponUsa__order__2645B050");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__CD65CB8558E70E54");

            entity.HasIndex(e => e.Email, "UQ__Customer__AB6E616417398904").IsUnique();

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
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("User")
                .HasColumnName("role");
        });

        modelBuilder.Entity<FeaturedProduct>(entity =>
        {
            entity.HasKey(e => e.FeaturedId).HasName("PK__Featured__8D1458B29BACEA10");

            entity.HasIndex(e => e.ProductId, "UQ__Featured__47027DF4D117BA27").IsUnique();

            entity.Property(e => e.FeaturedId).HasColumnName("featured_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("start_date");

            entity.HasOne(d => d.Product).WithOne(p => p.FeaturedProduct)
                .HasForeignKey<FeaturedProduct>(d => d.ProductId)
                .HasConstraintName("FK__FeaturedP__produ__04E4BC85");
        });


        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__465962296E97EBC4");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Updated_at");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CouponId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Orders__coupon_i__1332DBDC");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Orders__customer__123EB7A3");
        });

        modelBuilder.Entity<OrderCancellation>(entity =>
        {
            entity.HasKey(e => e.CancellationId).HasName("PK__OrderCan__4ED4366D78D36FD0");

            entity.HasIndex(e => e.OrderId, "UQ__OrderCan__465962282AAD4B47").IsUnique();

            entity.Property(e => e.CancellationId).HasColumnName("cancellation_id");
            entity.Property(e => e.CanceledAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("canceled_at");
            entity.Property(e => e.CanceledByUserId).HasColumnName("canceled_by_user_id");
            entity.Property(e => e.CustomReasonText)
                .HasMaxLength(500)
                .HasColumnName("custom_reason_text");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ReasonId).HasColumnName("reason_id");

            entity.HasOne(d => d.Order).WithOne(p => p.OrderCancellation)
                .HasForeignKey<OrderCancellation>(d => d.OrderId)
                .HasConstraintName("FK__OrderCanc__order__2FCF1A8A");

            entity.HasOne(d => d.Reason).WithMany(p => p.OrderCancellations)
                .HasForeignKey(d => d.ReasonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderCanc__reaso__30C33EC3");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__3764B6BCD32E828D");

            entity.HasIndex(e => e.VehicleId, "UQ__OrderIte__F2947BC0FE40155C").IsUnique();

            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UnitPriceAtPurchase)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("unit_price_at_purchase");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__order__17036CC0");

            entity.HasOne(d => d.Vehicle).WithOne(p => p.OrderItem)
                .HasForeignKey<OrderItem>(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__vehic__17F790F9");
        });

        modelBuilder.Entity<OrderShippingDetail>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__OrderShi__46596229BE1E585B");

            entity.Property(e => e.OrderId)
                .ValueGeneratedNever()
                .HasColumnName("order_id");
            entity.Property(e => e.CustomerNote)
                .HasMaxLength(500)
                .HasColumnName("customer_note");
            entity.Property(e => e.RecipientName)
                .HasMaxLength(100)
                .HasColumnName("recipient_name");
            entity.Property(e => e.RecipientPhone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("recipient_phone");
            entity.Property(e => e.StreetAddress)
                .HasMaxLength(255)
                .HasColumnName("street_address");

            entity.HasOne(d => d.Order).WithOne(p => p.OrderShippingDetail)
                .HasForeignKey<OrderShippingDetail>(d => d.OrderId)
                .HasConstraintName("FK__OrderShip__order__1AD3FDA4");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__ED1FC9EAD0E26A25");

            entity.HasIndex(e => e.OrderId, "UQ__Payments__46596228E7BB7E94").IsUnique();

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("idempotency_key");
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
            entity.Property(e => e.ProviderTransactionId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("provider_transaction_id");

            entity.HasOne(d => d.Order).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.OrderId)
                .HasConstraintName("FK__Payments__order___1F98B2C1");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__47027DF571FE51A3");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.BasePrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("base_price");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Products__brand___6B24EA82");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Products__catego__6A30C649");

            entity.HasMany(d => d.Promotions).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductPromotion",
                    r => r.HasOne<Promotion>().WithMany()
                        .HasForeignKey("PromotionId")
                        .HasConstraintName("FK__ProductPr__promo__7E37BEF6"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .HasConstraintName("FK__ProductPr__produ__7D439ABD"),
                    j =>
                    {
                        j.HasKey("ProductId", "PromotionId").HasName("PK__ProductP__E5C9E8A3176553E3");
                        j.ToTable("ProductPromotions");
                        j.IndexerProperty<int>("ProductId").HasColumnName("product_id");
                        j.IndexerProperty<int>("PromotionId").HasColumnName("promotion_id");
                    });
        });

        modelBuilder.Entity<ProductColor>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PK__ProductC__1143CECB6E200D03");

            entity.Property(e => e.ColorId).HasColumnName("color_id");
            entity.Property(e => e.ColorName)
                .HasMaxLength(50)
                .HasColumnName("color_name");
            entity.Property(e => e.PriceAdjustment)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("price_adjustment");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductColors)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductCo__produ__6EF57B66");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ProductI__DC9AC9559013B84F");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ColorId).HasColumnName("color_id");
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

            entity.HasOne(d => d.Color).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ColorId)
                .HasConstraintName("FK__ProductIm__color__0B91BA14");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductIm__produ__0A9D95DB");
        });

        modelBuilder.Entity<ProductSpecification>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.SpecId }).HasName("PK__ProductS__286571A3D311CDF4");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SpecId).HasColumnName("spec_id");
            entity.Property(e => e.SpecValue)
                .HasMaxLength(500)
                .HasColumnName("spec_value");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductSpecifications)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductSp__produ__797309D9");

            entity.HasOne(d => d.Spec).WithMany(p => p.ProductSpecifications)
                .HasForeignKey(d => d.SpecId)
                .HasConstraintName("FK__ProductSp__spec___7A672E12");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__2CB9556B33A294AD");

            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("discount_type");
            entity.Property(e => e.DiscountValue)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("discount_value");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
        });

        modelBuilder.Entity<Specification>(entity =>
        {
            entity.HasKey(e => e.SpecId).HasName("PK__Specific__F670C567CB735063");

            entity.HasIndex(e => e.SpecName, "UQ__Specific__B99801B18699C329").IsUnique();

            entity.Property(e => e.SpecId).HasColumnName("spec_id");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.SpecName)
                .HasMaxLength(255)
                .HasColumnName("spec_name");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__F2947BC1167A0B0E");

            entity.HasIndex(e => e.EngineNumber, "UQ__Vehicles__C2BF34ECD37E13E4").IsUnique();

            entity.HasIndex(e => e.Vin, "UQ__Vehicles__DDB00C663258CFF9").IsUnique();

            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.ColorId).HasColumnName("color_id");
            entity.Property(e => e.EngineNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("engine_number");
            entity.Property(e => e.ImportedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("imported_at");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken()
                .HasColumnName("row_version");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Available")
                .HasColumnName("status");
            entity.Property(e => e.Vin)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("vin");

            entity.HasOne(d => d.Color).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.ColorId)
                .HasConstraintName("FK__Vehicles__color___76969D2E");
        });

        modelBuilder.Entity<WarrantyBook>(entity =>
        {
            entity.HasKey(e => e.WarrantyId).HasName("PK__Warranty__24E65B0403693F9F");

            entity.HasIndex(e => e.VehicleId, "UQ__Warranty__F2947BC0615896DA").IsUnique();

            entity.Property(e => e.WarrantyId).HasColumnName("warranty_id");
            entity.Property(e => e.ActivatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("activated_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.ValidUntil)
                .HasColumnType("datetime")
                .HasColumnName("valid_until");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.WarrantyBooks)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WarrantyB__custo__3C34F16F");

            entity.HasOne(d => d.Vehicle).WithOne(p => p.WarrantyBook)
                .HasForeignKey<WarrantyBook>(d => d.VehicleId)
                .HasConstraintName("FK__WarrantyB__vehic__3B40CD36");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
