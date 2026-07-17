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
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");

            // ایندکس‌ها
            builder.HasIndex(v => v.ProductId)
                .HasDatabaseName("IX_ProductVariants_ProductId");

            builder.HasIndex(v => v.IsActive)
                .HasDatabaseName("IX_ProductVariants_IsActive");

            // تنظیم فیلدها
            builder.Property(v => v.ProductId)
                .IsRequired();

            builder.Property(v => v.VariantName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(v => v.VariantAttributes)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.Property(v => v.Quantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(v => v.PriceAdjustment)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(v => v.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(v => v.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(v => v.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // یک تنوع می‌تواند در چند CartItem و OrderItem باشد
            builder.HasMany(v => v.CartItems)
                .WithOne(ci => ci.Variant)
                .HasForeignKey(ci => ci.VariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(v => v.OrderItems)
                .WithOne(oi => oi.Variant)
                .HasForeignKey(oi => oi.VariantId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
