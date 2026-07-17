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
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            // ایندکس‌ها
            builder.HasIndex(oi => oi.OrderId)
                .HasDatabaseName("IX_OrderItems_OrderId");

            builder.HasIndex(oi => oi.ProductId)
                .HasDatabaseName("IX_OrderItems_ProductId");

            builder.HasIndex(oi => oi.VariantId)
                .HasDatabaseName("IX_OrderItems_VariantId");

            // تنظیم فیلدها
            builder.Property(oi => oi.OrderId)
                .IsRequired();

            builder.Property(oi => oi.ProductId)
                .IsRequired();

            builder.Property(oi => oi.VariantId)
                .IsRequired(false);

            builder.Property(oi => oi.ProductName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            builder.Property(oi => oi.ProductImage)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(oi => oi.VariantName)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            builder.Property(oi => oi.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(oi => oi.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(oi => oi.DiscountAmount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(oi => oi.FinalUnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(oi => oi.TotalPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(oi => oi.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(oi => oi.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(oi => oi.Variant)
                .WithMany(v => v.OrderItems)
                .HasForeignKey(oi => oi.VariantId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
