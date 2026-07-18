using JewelryStore.Data.Context;
using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.Report;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JewelryStore.Services.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1️⃣ آمار کلی داشبورد
        public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
            var newUsersToday = await _context.Users.CountAsync(u => u.IsActive && u.CreatedAt >= today);

            var totalOrders = await _context.Orders.CountAsync();
            var ordersToday = await _context.Orders.CountAsync(o => o.CreatedAt >= today);

            // سفارشات با وضعیت Pending (در انتظار پرداخت)
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.OrderStatus == OrderStatus.Pending);

            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var outOfStockProducts = await _context.Products
                .CountAsync(p => p.IsActive && p.Quantity <= 0);

            // محاسبه درآمد (فقط سفارشات با وضعیت Paid, Shipped, Delivered)
            var revenue = await _context.Orders
                .Where(o => o.OrderStatus != OrderStatus.Pending && o.OrderStatus != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            var revenueToday = await _context.Orders
                .Where(o => o.CreatedAt >= today && o.OrderStatus != OrderStatus.Pending && o.OrderStatus != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            var revenueThisMonth = await _context.Orders
                .Where(o => o.CreatedAt >= startOfMonth && o.OrderStatus != OrderStatus.Pending && o.OrderStatus != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            return new DashboardStatisticsDto
            {
                TotalUsers = totalUsers,
                NewUsersToday = newUsersToday,
                TotalOrders = totalOrders,
                OrdersToday = ordersToday,
                TotalProducts = totalProducts,
                OutOfStockProducts = outOfStockProducts,
                TotalRevenue = revenue,
                RevenueToday = revenueToday,
                RevenueThisMonth = revenueThisMonth,
                PendingOrders = pendingOrders
            };
        }

        // 2️⃣ گزارش فروش در بازه زمانی
        public async Task<IEnumerable<SalesReportDto>> GetSalesReportAsync(
            DateTime fromDate, DateTime toDate, string groupBy = "day")
        {
            // اعمال فیلتر تاریخ و وضعیت
            var ordersQuery = _context.Orders
                .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
                .Where(o => o.OrderStatus != OrderStatus.Pending && o.OrderStatus != OrderStatus.Cancelled);

            // گروه‌بندی بر اساس بازه زمانی
            var groupedQuery = groupBy.ToLower() switch
            {
                "month" => ordersQuery
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .Select(g => new
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        OrderCount = g.Count(),
                        TotalAmount = g.Sum(o => o.TotalAmount)
                    }),
                "year" => ordersQuery
                    .GroupBy(o => o.CreatedAt.Year)
                    .Select(g => new
                    {
                        Date = new DateTime(g.Key, 1, 1),
                        OrderCount = g.Count(),
                        TotalAmount = g.Sum(o => o.TotalAmount)
                    }),
                _ => ordersQuery
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        OrderCount = g.Count(),
                        TotalAmount = g.Sum(o => o.TotalAmount)
                    })
            };

            var result = await groupedQuery
                .OrderBy(g => g.Date)
                .Select(g => new SalesReportDto
                {
                    Date = g.Date,
                    OrderCount = g.OrderCount,
                    TotalAmount = g.TotalAmount,
                    AverageOrderValue = g.OrderCount > 0 ? g.TotalAmount / g.OrderCount : 0
                })
                .ToListAsync();

            return result;
        }

        // 3️⃣ پرفروش‌ترین محصولات
        public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(
            int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var orderItemsQuery = _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Images)
                .AsQueryable();

            // اعمال فیلتر تاریخ (اگر ارسال شده باشد)
            if (fromDate.HasValue && toDate.HasValue)
            {
                orderItemsQuery = orderItemsQuery
                    .Where(oi => oi.CreatedAt >= fromDate.Value && oi.CreatedAt <= toDate.Value);
            }

            var topProducts = await orderItemsQuery
                .GroupBy(oi => new { oi.ProductId, oi.Product })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Product.Name,
                    MainImageUrl = g.Key.Product.Images.FirstOrDefault(i => i.IsMain).ImageUrl,
                    CategoryId = g.Key.Product.CategoryId,
                    CategoryName = g.Key.Product.Category.Name,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(count)
                .ToListAsync();

            return topProducts;
        }

        // 4️⃣ مشتریان وفادار
        public async Task<IEnumerable<TopUserDto>> GetTopUsersAsync(
            int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Where(o => o.OrderStatus != OrderStatus.Pending && o.OrderStatus != OrderStatus.Cancelled)
                .AsQueryable();

            // اعمال فیلتر تاریخ
            if (fromDate.HasValue && toDate.HasValue)
            {
                ordersQuery = ordersQuery
                    .Where(o => o.CreatedAt >= fromDate.Value && o.CreatedAt <= toDate.Value);
            }

            var topUsers = await ordersQuery
                .GroupBy(o => new { o.UserId, o.User })
                .Select(g => new TopUserDto
                {
                    UserId = g.Key.UserId,
                    FullName = g.Key.User.FullName ?? g.Key.User.Username,
                    PhoneNumber = g.Key.User.PhoneNumber,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    LastOrderDate = g.Max(o => o.CreatedAt)
                })
                .OrderByDescending(u => u.TotalSpent)
                .Take(count)
                .ToListAsync();

            return topUsers;
        }

        // 5️⃣ توزیع وضعیت سفارش‌ها
        public async Task<IEnumerable<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync()
        {
            var totalOrders = await _context.Orders.CountAsync();

            var distribution = await _context.Orders
                .GroupBy(o => o.OrderStatus)
                .Select(g => new OrderStatusDistributionDto
                {
                    Status = g.Key,
                    StatusName = GetStatusName(g.Key),
                    Count = g.Count(),
                    Percentage = totalOrders > 0 ? (g.Count() * 100 / totalOrders) : 0
                })
                .OrderByDescending(d => d.Count)
                .ToListAsync();

            return distribution;
        }

        // 6️⃣ گزارش فروش به تفکیک دسته‌بندی
        public async Task<IEnumerable<CategorySalesReportDto>> GetCategorySalesReportAsync(
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            var orderItemsQuery = _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .AsQueryable();

            // اعمال فیلتر تاریخ
            if (fromDate.HasValue && toDate.HasValue)
            {
                orderItemsQuery = orderItemsQuery
                    .Where(oi => oi.CreatedAt >= fromDate.Value && oi.CreatedAt <= toDate.Value);
            }

            var report = await orderItemsQuery
                .GroupBy(oi => new { oi.Product.CategoryId, oi.Product.Category.Name })
                .Select(g => new CategorySalesReportDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice),
                    OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ToListAsync();

            return report;
        }

        // 🔧 متد کمکی برای ترجمه وضعیت
        private string GetStatusName(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "در انتظار پرداخت",
                OrderStatus.Paid => "پرداخت شده",
                OrderStatus.Processing => "در حال پردازش",
                OrderStatus.Shipped => "ارسال شده",
                OrderStatus.Delivered => "تحویل داده شده",
                OrderStatus.Cancelled => "لغو شده",
                OrderStatus.Returned => "مرجوع شده",
                _ => "نامشخص"
            };
        }
    }
}
