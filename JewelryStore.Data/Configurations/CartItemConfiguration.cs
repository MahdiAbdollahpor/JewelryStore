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
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");

            // ایندکس‌ها
            builder.HasIndex(ci => ci.CartId)
                .HasDatabaseName("IX_CartItems_CartId");

            builder.HasIndex(ci => ci.ProductId)
                .HasDatabaseName("IX_CartItems_ProductId");

            builder.HasIndex(ci => ci.VariantId)
                .HasDatabaseName("IX_CartItems_VariantId");

            // Unique Constraint برای جلوگیری از تکراری شدن
            builder.HasIndex(ci => new { ci.CartId, ci.ProductId, ci.VariantId })
                .IsUnique()
                .HasDatabaseName("IX_CartItems_CartId_ProductId_VariantId");

            // تنظیم فیلدها
            builder.Property(ci => ci.CartId)
                .IsRequired();

            builder.Property(ci => ci.ProductId)
                .IsRequired();

            builder.Property(ci => ci.VariantId)
                .IsRequired(false);

            builder.Property(ci => ci.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(ci => ci.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(ci => ci.DiscountAmount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(ci => ci.FinalUnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(ci => ci.TotalPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(ci => ci.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(ci => ci.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ci => ci.Variant)
                .WithMany(v => v.CartItems)
                .HasForeignKey(ci => ci.VariantId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
