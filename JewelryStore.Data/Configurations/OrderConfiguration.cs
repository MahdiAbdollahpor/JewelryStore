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
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            // ایندکس‌ها
            builder.HasIndex(o => o.OrderNumber)
                .IsUnique()
                .HasDatabaseName("IX_Orders_OrderNumber");

            builder.HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Orders_UserId");

            builder.HasIndex(o => o.OrderStatus)
                .HasDatabaseName("IX_Orders_OrderStatus");

            builder.HasIndex(o => o.PaymentStatus)
                .HasDatabaseName("IX_Orders_PaymentStatus");

            builder.HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IX_Orders_CreatedAt");

            // ایندکس ترکیبی برای جستجو
            builder.HasIndex(o => new { o.UserId, o.OrderStatus })
                .HasDatabaseName("IX_Orders_UserId_OrderStatus");

            // تنظیم فیلدها
            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            builder.Property(o => o.UserId)
                .IsRequired();

            builder.Property(o => o.OrderStatus)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(o => o.PaymentStatus)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(o => o.PaymentMethod)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(o => o.PaymentReference)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            builder.Property(o => o.PaymentDate)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(o => o.SubTotal)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(o => o.DiscountTotal)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(o => o.ShippingCost)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(o => o.TaxAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(o => o.DiscountCodeId)
                .IsRequired(false);

            builder.Property(o => o.DiscountCodeAmount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(o => o.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.Property(o => o.RecipientName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(o => o.RecipientPhone)
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnType("nvarchar(11)");

            builder.Property(o => o.ShippingDate)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(o => o.DeliveryDate)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(o => o.TrackingCode)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)")
                .IsRequired(false);

            builder.Property(o => o.CustomerNote)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(o => o.AdminNote)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(o => o.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(o => o.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.DiscountCode)
                .WithMany()
                .HasForeignKey(o => o.DiscountCodeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.StatusHistory)
                .WithOne(sh => sh.Order)
                .HasForeignKey(sh => sh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.PaymentTransactions)
                .WithOne(pt => pt.Order)
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
