using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.Discount;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JewelryStore.Services.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1️⃣ اعتبارسنجی و اعمال کد تخفیف
        public async Task<DiscountValidationResult> ValidateAndApplyDiscountAsync(
    string code, int userId, decimal orderAmount)
        {
            try
            {
                // ✅ بررسی وجود کد تخفیف
                var discountCode = await _context.DiscountCodes
                    .FirstOrDefaultAsync(d => d.Code == code && d.IsActive);

                if (discountCode == null)
                    return new DiscountValidationResult
                    {
                        IsValid = false,
                        Message = "کد تخفیف نامعتبر است."
                    };

                // بررسی تاریخ انقضا
                if (discountCode.EndDate.HasValue && discountCode.EndDate.Value < DateTime.Now)
                    return new DiscountValidationResult
                    {
                        IsValid = false,
                        Message = "کد تخفیف منقضی شده است."
                    };

                if (discountCode.StartDate.HasValue && discountCode.StartDate.Value > DateTime.Now)
                    return new DiscountValidationResult
                    {
                        IsValid = false,
                        Message = "کد تخفیف هنوز فعال نشده است."
                    };

                // بررسی حداقل مبلغ سفارش
                if (discountCode.MinOrderAmount.HasValue && orderAmount < discountCode.MinOrderAmount.Value)
                    return new DiscountValidationResult
                    {
                        IsValid = false,
                        Message = $"حداقل مبلغ سفارش برای این کد تخفیف {discountCode.MinOrderAmount.Value:N0} تومان است."
                    };

                // بررسی محدودیت تعداد استفاده کلی
                if (discountCode.UsageLimit.HasValue && discountCode.UsedCount >= discountCode.UsageLimit.Value)
                    return new DiscountValidationResult
                    {
                        IsValid = false,
                        Message = "تعداد استفاده از این کد تخفیف به پایان رسیده است."
                    };

                // بررسی محدودیت استفاده برای هر کاربر
                if (discountCode.UsagePerUser.HasValue)
                {
                    var userUsageCount = await _context.DiscountUsages
                        .CountAsync(u => u.UserId == userId && u.DiscountCodeId == discountCode.Id);

                    if (userUsageCount >= discountCode.UsagePerUser.Value)
                        return new DiscountValidationResult
                        {
                            IsValid = false,
                            Message = "شما قبلاً از این کد تخفیف استفاده کرده‌اید."
                        };
                }

                // محاسبه مبلغ تخفیف
                decimal discountAmount = 0;
                if (discountCode.DiscountType == DiscountType.Percentage)
                {
                    discountAmount = orderAmount * (discountCode.DiscountValue / 100);
                    if (discountCode.MaxDiscountAmount.HasValue)
                        discountAmount = Math.Min(discountAmount, discountCode.MaxDiscountAmount.Value);
                }
                else // FixedAmount
                {
                    discountAmount = discountCode.DiscountValue;
                    if (discountAmount > orderAmount)
                        discountAmount = orderAmount;
                }

                return new DiscountValidationResult
                {
                    IsValid = true,
                    Message = "کد تخفیف با موفقیت اعمال شد.",
                    DiscountCode = discountCode,
                    DiscountAmount = discountAmount
                };
            }
            catch (Exception ex)
            {
                // ✅ لاگ خطا
                return new DiscountValidationResult
                {
                    IsValid = false,
                    Message = $"خطا در اعتبارسنجی کد تخفیف: {ex.Message}"
                };
            }
        }

        // 2️⃣ ثبت استفاده از کد تخفیف
        public async Task<bool> RecordDiscountUsageAsync(int discountCodeId, int userId, int orderId)
        {
            var usage = new DiscountUsage
            {
                DiscountCodeId = discountCodeId,
                UserId = userId,
                OrderId = orderId,
                UsedAt = DateTime.Now
            };

            await _context.DiscountUsages.AddAsync(usage);

            // افزایش شمارنده استفاده
            var discountCode = await _context.DiscountCodes.FindAsync(discountCodeId);
            if (discountCode != null)
            {
                discountCode.UsedCount++;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
