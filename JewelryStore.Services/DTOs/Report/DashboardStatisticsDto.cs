using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Report
{
    public class DashboardStatisticsDto
    {
        // کاربران
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }

        // سفارشات
        public int TotalOrders { get; set; }
        public int OrdersToday { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }

        // درآمد
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // محصولات
        public int TotalProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int LowStockProducts { get; set; }
    }
}
