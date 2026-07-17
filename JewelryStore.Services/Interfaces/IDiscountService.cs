using JewelryStore.Services.DTOs.Discount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface IDiscountService
    {
        /// اعتبارسنجی و محاسبه مبلغ تخفیف برای یک کد
        Task<DiscountValidationResult> ValidateAndApplyDiscountAsync(
            string code, int userId, decimal orderAmount);

        /// ثبت استفاده از کد تخفیف در پایگاه داده
        Task<bool> RecordDiscountUsageAsync(int discountCodeId, int userId, int orderId);
    }
}
