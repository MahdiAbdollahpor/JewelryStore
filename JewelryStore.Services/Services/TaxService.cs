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
    public class TaxService : ITaxService
    {
        private readonly ApplicationDbContext _context;

        public TaxService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateTaxAsync(decimal amount)
        {
            var settings = await _context.TaxSettings
                .FirstOrDefaultAsync(t => t.IsActive);

            if (settings == null)
                return 0;

            return amount * (settings.TaxPercentage / 100);
        }
    }
}
