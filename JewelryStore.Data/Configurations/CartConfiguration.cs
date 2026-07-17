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
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");

            // ایندکس‌ها
            builder.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Carts_UserId");

            builder.HasIndex(c => c.SessionId)
                .HasDatabaseName("IX_Carts_SessionId");

            // تنظیم فیلدها
            builder.Property(c => c.UserId)
                .IsRequired(false);

            builder.Property(c => c.SessionId)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            builder.Property(c => c.ExpiryDate)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
