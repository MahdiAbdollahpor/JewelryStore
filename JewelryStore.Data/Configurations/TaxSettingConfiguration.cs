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
    public class TaxSettingConfiguration : IEntityTypeConfiguration<TaxSetting>
    {
        public void Configure(EntityTypeBuilder<TaxSetting> builder)
        {
            builder.ToTable("TaxSettings");

            builder.HasIndex(t => t.IsActive)
                .HasDatabaseName("IX_TaxSettings_IsActive");

            // تنظیم فیلدها
            builder.Property(t => t.TaxPercentage)
                .IsRequired()
                .HasPrecision(5, 2);

            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(t => t.UpdatedByUserId)
                .IsRequired(false);

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(t => t.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(t => t.UpdatedByUser)
                .WithMany()
                .HasForeignKey(t => t.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
