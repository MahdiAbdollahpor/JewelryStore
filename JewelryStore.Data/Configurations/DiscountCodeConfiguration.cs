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
    public class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
    {
        public void Configure(EntityTypeBuilder<DiscountCode> builder)
        {
            builder.ToTable("DiscountCodes");

            // ایندکس‌ها
            builder.HasIndex(d => d.Code)
                .IsUnique()
                .HasDatabaseName("IX_DiscountCodes_Code");

            builder.HasIndex(d => d.IsActive)
                .HasDatabaseName("IX_DiscountCodes_IsActive");

            builder.HasIndex(d => d.EndDate)
                .HasDatabaseName("IX_DiscountCodes_EndDate");

            // تنظیم فیلدها
            builder.Property(d => d.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(d => d.Title)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(d => d.DiscountType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(d => d.DiscountValue)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(d => d.MaxDiscountAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            builder.Property(d => d.MinOrderAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            builder.Property(d => d.UsageLimit)
                .IsRequired(false);

            builder.Property(d => d.UsagePerUser)
                .IsRequired(false);

            builder.Property(d => d.UsedCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(d => d.StartDate)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(d => d.EndDate)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(d => d.ApplicableProducts)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property(d => d.ApplicableCategories)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property(d => d.ExcludedProducts)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property(d => d.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(d => d.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasMany(d => d.Usages)
                .WithOne(u => u.DiscountCode)
                .HasForeignKey(u => u.DiscountCodeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
