using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface IShippingService
    {
        /// محاسبه هزینه ارسال بر اساس مبلغ سفارش و تنظیمات
        Task<decimal> CalculateShippingCostAsync(decimal orderAmount);
    }
}
