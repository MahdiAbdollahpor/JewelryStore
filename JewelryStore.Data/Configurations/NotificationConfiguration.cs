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
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            // ایندکس‌ها
            builder.HasIndex(n => n.UserId)
                .HasDatabaseName("IX_Notifications_UserId");

            builder.HasIndex(n => n.OrderId)
                .HasDatabaseName("IX_Notifications_OrderId");

            builder.HasIndex(n => n.IsSent)
                .HasDatabaseName("IX_Notifications_IsSent");

            builder.HasIndex(n => n.CreatedAt)
                .HasDatabaseName("IX_Notifications_CreatedAt");

            // تنظیم فیلدها
            builder.Property(n => n.UserId)
                .IsRequired(false);

            builder.Property(n => n.OrderId)
                .IsRequired();

            builder.Property(n => n.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.Property(n => n.PhoneNumber)
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnType("nvarchar(11)");

            builder.Property(n => n.IsSent)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.SentAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(n => n.Error)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(n => n.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(n => n.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.Order)
                .WithMany()
                .HasForeignKey(n => n.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
