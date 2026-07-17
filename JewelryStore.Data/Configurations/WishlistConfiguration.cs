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
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.ToTable("Wishlists");

            // ایندکس‌ها
            builder.HasIndex(w => w.UserId)
                .HasDatabaseName("IX_Wishlists_UserId");

            builder.HasIndex(w => w.ProductId)
                .HasDatabaseName("IX_Wishlists_ProductId");

            // Unique Constraint برای جلوگیری از تکراری شدن
            builder.HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_Wishlists_UserId_ProductId");

            // تنظیم فیلدها
            builder.Property(w => w.UserId)
                .IsRequired();

            builder.Property(w => w.ProductId)
                .IsRequired();

            builder.Property(w => w.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(w => w.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
