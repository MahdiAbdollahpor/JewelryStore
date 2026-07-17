using JewelryStore.Data.Context;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Services
{
    public class ShippingService : IShippingService
    {
        private readonly ApplicationDbContext _context;

        public ShippingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateShippingCostAsync(decimal orderAmount)
        {
            var settings = await _context.ShippingSettings
                .FirstOrDefaultAsync(s => s.IsActive);

            if (settings == null)
                return 0;

            // اگر مبلغ سفارش بیشتر از آستانه ارسال رایگان باشد
            if (settings.FreeShippingThreshold.HasValue &&
                orderAmount >= settings.FreeShippingThreshold.Value)
            {
                return 0;
            }

            return settings.ShippingCost;
        }
    }
}
