using JewelryStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryStore.Data.Configurations
{
    public class CategoryAttributeConfiguration : IEntityTypeConfiguration<CategoryAttribute>
    {
        public void Configure(EntityTypeBuilder<CategoryAttribute> builder)
        {
            builder.ToTable("CategoryAttributes");

            // ایندکس‌ها
            builder.HasIndex(a => a.CategoryId)
                .HasDatabaseName("IX_CategoryAttributes_CategoryId");

            builder.HasIndex(a => new { a.CategoryId, a.Name })
                .IsUnique()
                .HasDatabaseName("IX_CategoryAttributes_CategoryId_Name");

            // تنظیم فیلدها
            builder.Property(a => a.CategoryId)
                .IsRequired();

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(a => a.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(a => a.IsRequired)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(a => a.IsFilterable)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(a => a.Options)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(a => a.Category)
                .WithMany(c => c.Attributes)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.ProductAttributeValues)
                .WithOne(p => p.Attribute)
                .HasForeignKey(p => p.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
