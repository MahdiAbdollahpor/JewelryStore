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
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("OrderStatusHistories");

            // ایندکس‌ها
            builder.HasIndex(sh => sh.OrderId)
                .HasDatabaseName("IX_OrderStatusHistories_OrderId");

            builder.HasIndex(sh => sh.Status)
                .HasDatabaseName("IX_OrderStatusHistories_Status");

            builder.HasIndex(sh => sh.CreatedAt)
                .HasDatabaseName("IX_OrderStatusHistories_CreatedAt");

            // تنظیم فیلدها
            builder.Property(sh => sh.OrderId)
                .IsRequired();

            builder.Property(sh => sh.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(sh => sh.Note)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(sh => sh.ChangedByUserId)
                .IsRequired(false);

            builder.Property(sh => sh.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(sh => sh.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(sh => sh.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(sh => sh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sh => sh.ChangedByUser)
                .WithMany()
                .HasForeignKey(sh => sh.ChangedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
