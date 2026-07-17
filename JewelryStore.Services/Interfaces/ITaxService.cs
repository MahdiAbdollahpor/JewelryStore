using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface ITaxService
    {
        /// محاسبه مالیات بر اساس مبلغ و درصد مالیات فعال
        /// </summary>
        Task<decimal> CalculateTaxAsync(decimal amount);
    }
}
