using JewelryStore.Domain.Entities;
using JewelryStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryStore.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // 1️⃣ تنظیم نام جدول (اختیاری - اگر بخواهیم نام پیش‌فرض را تغییر دهیم)
            builder.ToTable("Users", "dbo");

            // 2️⃣ تنظیم کلید اصلی (اختیاری - زیرا از BaseEntity ارث‌بری کرده)
            // builder.HasKey(u => u.Id);

            // 3️⃣ تنظیم ایندکس‌ها (برای بهبود سرعت جستجو)
            builder.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("IX_Users_Username");

            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique()
                .HasDatabaseName("IX_Users_PhoneNumber");

            builder.HasIndex(u => u.Role)
                .HasDatabaseName("IX_Users_Role");

            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            // 4️⃣ تنظیم ویژگی‌های هر فیلد
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("Username")
                .HasColumnOrder(1)
                .HasColumnType("nvarchar(50)");

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnName("PhoneNumber")
                .HasColumnOrder(2)
                .HasColumnType("nvarchar(11)")
                .HasComment("شماره تماس - یکتا و برای ورود استفاده می‌شود");

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("PasswordHash")
                .HasColumnOrder(3)
                .HasColumnType("nvarchar(500)")
                .HasComment("رمز عبور هش شده (هرگز به صورت ساده ذخیره نمی‌شود)");

            builder.Property(u => u.FullName)
                .HasMaxLength(100)
                .HasColumnName("FullName")
                .HasColumnOrder(4)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false)
                .HasComment("نام و نام خانوادگی - اختیاری");

            builder.Property(u => u.Role)
                .IsRequired()
                .HasColumnName("Role")
                .HasColumnOrder(5)
                .HasDefaultValue(UserRole.User)
                .HasConversion<int>()
                .HasComment("نقش کاربر: 0 = User, 1 = Admin");

            builder.Property(u => u.IsPhoneVerified)
                .IsRequired()
                .HasColumnName("IsPhoneVerified")
                .HasColumnOrder(6)
                .HasDefaultValue(false)
                .HasComment("آیا شماره تماس تایید شده است؟");

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasColumnName("IsActive")
                .HasColumnOrder(7)
                .HasDefaultValue(true)
                .HasComment("آیا حساب کاربری فعال است؟");

            builder.Property(u => u.Address)
                .HasMaxLength(500)
                .HasColumnName("Address")
                .HasColumnOrder(8)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false)
                .HasComment("آدرس - در زمان سفارش اجباری می‌شود");

            builder.Property(u => u.LastLoginAt)
                .HasColumnName("LastLoginAt")
                .HasColumnOrder(9)
                .HasColumnType("datetime2")
                .IsRequired(false)
                .HasComment("تاریخ آخرین ورود");

            // 5️⃣ تنظیم فیلدهای پایه (از BaseEntity)
            builder.Property(u => u.Id)
                .HasColumnName("Id")
                .HasColumnOrder(0)
                .UseIdentityColumn(1, 1)
                .HasComment("شناسه یکتا");

            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt")
                .HasColumnOrder(10)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .HasComment("تاریخ ایجاد");

            builder.Property(u => u.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .HasColumnOrder(11)
                .HasColumnType("datetime2")
                .IsRequired(false)
                .HasComment("تاریخ آخرین ویرایش");

            // 6️⃣ تنظیم روابط (Relationships)

            // رابطه یک به چند با Order
            builder.HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict) // جلوگیری از حذف کاربری که سفارش دارد
                .HasConstraintName("FK_Orders_Users_UserId");

            // رابطه یک به چند با Wishlist
            builder.HasMany(u => u.Wishlists)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade) // با حذف کاربر، علاقه‌مندی‌ها هم حذف شوند
                .HasConstraintName("FK_Wishlists_Users_UserId");

            // رابطه یک به چند با Cart
            builder.HasMany(u => u.Carts)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Carts_Users_UserId");

            // رابطه یک به چند با DiscountUsage
            builder.HasMany(u => u.DiscountUsages)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DiscountUsages_Users_UserId");

            // رابطه یک به چند با Notification
            builder.HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Notifications_Users_UserId");

            // 7️⃣ تنظیم فیلتر Global Query (اختیاری - برای نمایش فقط کاربران فعال به صورت پیش‌فرض)
            // builder.HasQueryFilter(u => u.IsActive);

            // 8️⃣ تنظیم داده‌های اولیه (Seed Data) برای کاربر ادمین
            // این کار را در SeedData.cs انجام خواهیم داد، نه در اینجا

            // 9️⃣ تنظیم تنظیمات دقیق‌تر
            builder.Property(u => u.PhoneNumber)
                .HasAnnotation("RegularExpression", @"^09\d{9}$")
                .HasComment("شماره تماس باید با 09 شروع و 11 رقم باشد");
        }
    }
}
