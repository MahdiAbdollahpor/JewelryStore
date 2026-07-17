using JewelryStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            // ایندکس‌ها
            builder.HasIndex(p => p.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Products_Slug");

            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Products_CategoryId");

            builder.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_Products_IsActive");

            builder.HasIndex(p => p.IsFeatured)
                .HasDatabaseName("IX_Products_IsFeatured");

            builder.HasIndex(p => p.Purity)
                .HasDatabaseName("IX_Products_Purity");

            builder.HasIndex(p => p.IsInStock)
                .HasDatabaseName("IX_Products_IsInStock");

            // ایندکس ترکیبی برای جستجو
            builder.HasIndex(p => new { p.IsActive, p.IsInStock, p.IsFeatured })
                .HasDatabaseName("IX_Products_Active_Stock_Featured");

            // تنظیم فیلدها
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            builder.Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            builder.Property(p => p.CategoryId)
                .IsRequired();

            builder.Property(p => p.Brand)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.ShortDescription)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(p => p.BasePrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.DiscountPercentage)
                .IsRequired()
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.Property(p => p.FinalPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.Weight)
                .IsRequired()
                .HasPrecision(10, 3);

            builder.Property(p => p.Purity)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.GoldPriceReference)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.CraftsmanshipFee)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.StoneType)
                .IsRequired(false)
                .HasConversion<int?>();

            builder.Property(p => p.StoneWeight)
                .HasPrecision(10, 3)
                .IsRequired(false);

            builder.Property(p => p.StoneQuality)
                .IsRequired(false)
                .HasConversion<int?>();

            builder.Property(p => p.Quantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.MinOrderQuantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(p => p.MaxOrderQuantity)
                .IsRequired()
                .HasDefaultValue(10);

            // IsInStock به عنوان فیلد محاسباتی
            builder.Property(p => p.IsInStock)
                .IsRequired()
                .HasComputedColumnSql("CASE WHEN Quantity > 0 THEN 1 ELSE 0 END", stored: true);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.IsNew)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.ViewCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.AverageRating)
                .IsRequired()
                .HasPrecision(3, 2)
                .HasDefaultValue(0);

            builder.Property(p => p.ReviewCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.PublishedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.ProductTags)
                .WithOne(pt => pt.Product)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.AttributeValues)
                .WithOne(av => av.Product)
                .HasForeignKey(av => av.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Wishlists)
                .WithOne(w => w.Product)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.CartItems)
                .WithOne(ci => ci.Product)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.OrderItems)
                .WithOne(oi => oi.Product)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
