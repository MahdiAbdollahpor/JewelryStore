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
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");

            // ایندکس‌ها
            builder.HasIndex(pt => pt.OrderId)
                .HasDatabaseName("IX_PaymentTransactions_OrderId");

            builder.HasIndex(pt => pt.TransactionId)
                .HasDatabaseName("IX_PaymentTransactions_TransactionId");

            builder.HasIndex(pt => pt.Status)
                .HasDatabaseName("IX_PaymentTransactions_Status");

            // تنظیم فیلدها
            builder.Property(pt => pt.OrderId)
                .IsRequired();

            builder.Property(pt => pt.TransactionId)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(pt => pt.PaymentMethod)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(pt => pt.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(pt => pt.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(pt => pt.GatewayResponse)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property(pt => pt.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(pt => pt.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // روابط
            builder.HasOne(pt => pt.Order)
                .WithMany(o => o.PaymentTransactions)
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
