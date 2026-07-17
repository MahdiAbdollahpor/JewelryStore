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
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");

            // ایندکس‌ها
            builder.HasIndex(i => i.ProductId)
                .HasDatabaseName("IX_ProductImages_ProductId");

            builder.HasIndex(i => i.IsMain)
                .HasDatabaseName("IX_ProductImages_IsMain");

            // تنظیم فیلدها
            builder.Property(i => i.ProductId)
                .IsRequired();

            builder.Property(i => i.ImageUrl)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.Property(i => i.AltText)
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)")
                .IsRequired(false);

            builder.Property(i => i.IsMain)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(i => i.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(i => i.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(i => i.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
