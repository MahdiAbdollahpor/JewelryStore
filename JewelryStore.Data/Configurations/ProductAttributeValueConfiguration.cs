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
    public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
    {
        public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
        {
            builder.ToTable("ProductAttributeValues");

            // ایندکس‌ها
            builder.HasIndex(pav => pav.ProductId)
                .HasDatabaseName("IX_ProductAttributeValues_ProductId");

            builder.HasIndex(pav => pav.AttributeId)
                .HasDatabaseName("IX_ProductAttributeValues_AttributeId");

            // Unique Constraint برای جلوگیری از تکراری شدن
            builder.HasIndex(pav => new { pav.ProductId, pav.AttributeId })
                .IsUnique()
                .HasDatabaseName("IX_ProductAttributeValues_ProductId_AttributeId");

            // تنظیم فیلدها
            builder.Property(pav => pav.ProductId)
                .IsRequired();

            builder.Property(pav => pav.AttributeId)
                .IsRequired();

            builder.Property(pav => pav.Value)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.Property(pav => pav.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(pav => pav.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(pav => pav.Product)
                .WithMany(p => p.AttributeValues)
                .HasForeignKey(pav => pav.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pav => pav.Attribute)
                .WithMany(a => a.ProductAttributeValues)
                .HasForeignKey(pav => pav.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
