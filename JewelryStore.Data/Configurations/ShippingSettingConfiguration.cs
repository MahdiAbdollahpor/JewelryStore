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
    public class ShippingSettingConfiguration : IEntityTypeConfiguration<ShippingSetting>
    {
        public void Configure(EntityTypeBuilder<ShippingSetting> builder)
        {
            builder.ToTable("ShippingSettings");

            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_ShippingSettings_IsActive");

            // تنظیم فیلدها
            builder.Property(s => s.ShippingCost)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(s => s.FreeShippingThreshold)
                .HasPrecision(18, 2)
                .IsRequired(false);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.UpdatedByUserId)
                .IsRequired(false);

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(s => s.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(s => s.UpdatedByUser)
                .WithMany()
                .HasForeignKey(s => s.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
