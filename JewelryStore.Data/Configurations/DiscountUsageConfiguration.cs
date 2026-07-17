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
    public class DiscountUsageConfiguration : IEntityTypeConfiguration<DiscountUsage>
    {
        public void Configure(EntityTypeBuilder<DiscountUsage> builder)
        {
            builder.ToTable("DiscountUsages");

            // ایندکس‌ها
            builder.HasIndex(u => u.DiscountCodeId)
                .HasDatabaseName("IX_DiscountUsages_DiscountCodeId");

            builder.HasIndex(u => u.UserId)
                .HasDatabaseName("IX_DiscountUsages_UserId");

            builder.HasIndex(u => u.OrderId)
                .HasDatabaseName("IX_DiscountUsages_OrderId");

            // Unique Constraint برای جلوگیری از استفاده تکراری
            builder.HasIndex(u => new { u.DiscountCodeId, u.UserId, u.OrderId })
                .IsUnique()
                .HasDatabaseName("IX_DiscountUsages_DiscountCodeId_UserId_OrderId");

            // تنظیم فیلدها
            builder.Property(u => u.DiscountCodeId)
                .IsRequired();

            builder.Property(u => u.UserId)
                .IsRequired();

            builder.Property(u => u.OrderId)
                .IsRequired();

            builder.Property(u => u.UsedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(u => u.DiscountCode)
                .WithMany(d => d.Usages)
                .HasForeignKey(u => u.DiscountCodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.User)
                .WithMany(us => us.DiscountUsages)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Order)
                .WithMany()
                .HasForeignKey(u => u.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
