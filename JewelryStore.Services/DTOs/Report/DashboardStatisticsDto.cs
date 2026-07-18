using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Report
{
    public class DashboardStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int TotalOrders { get; set; }
        public int OrdersToday { get; set; }
        public int TotalProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int PendingOrders { get; set; } // سفارشات در انتظار پرداخت
    }
}
