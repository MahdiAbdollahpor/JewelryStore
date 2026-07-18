using JewelryStore.Services.DTOs.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface IReportService
    {
        //  آمار کلی داشبورد
        Task<DashboardStatisticsDto> GetDashboardStatisticsAsync();

        //  گزارش فروش در بازه زمانی
        Task<IEnumerable<SalesReportDto>> GetSalesReportAsync(DateTime fromDate, DateTime toDate, string groupBy = "day");

        //  پرفروش‌ترین محصولات
        Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);

        //  مشتریان وفادار
        Task<IEnumerable<TopUserDto>> GetTopUsersAsync(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);

        //  توزیع وضعیت سفارش‌ها
        Task<IEnumerable<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync();

        //  گزارش فروش به تفکیک دسته‌بندی
        Task<IEnumerable<CategorySalesReportDto>> GetCategorySalesReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}
