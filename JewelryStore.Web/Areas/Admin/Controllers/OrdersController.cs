using JewelryStore.Services.DTOs.Admin;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IAdminService adminService, ILogger<OrdersController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AdminOrderFilterDto filter)
        {
            try
            {
                var orders = await _adminService.GetAllOrdersAsync(filter);
                ViewBag.CurrentFilter = filter;
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست سفارشات");
                TempData["Error"] = "خطا در دریافت لیست سفارشات.";
                return View(new List<OrderListDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var order = await _adminService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound();

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت جزئیات سفارش");
                TempData["Error"] = "خطا در دریافت جزئیات سفارش.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? note = null)
        {
            try
            {
                var result = await _adminService.UpdateOrderStatusAsync(id, status, note);
                if (result)
                    TempData["Success"] = "وضعیت سفارش با موفقیت تغییر کرد.";
                else
                    TempData["Error"] = "خطا در تغییر وضعیت سفارش.";

                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت سفارش");
                TempData["Error"] = "خطا در تغییر وضعیت سفارش.";
                return RedirectToAction("Detail", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTracking(int id, string trackingCode)
        {
            try
            {
                var result = await _adminService.AddTrackingCodeAsync(id, trackingCode);
                if (result)
                    TempData["Success"] = "کد رهگیری با موفقیت افزوده شد.";
                else
                    TempData["Error"] = "خطا در افزودن کد رهگیری.";

                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در افزودن کد رهگیری");
                TempData["Error"] = "خطا در افزودن کد رهگیری.";
                return RedirectToAction("Detail", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            try
            {
                var result = await _adminService.CancelOrderAsync(id, reason);
                if (result)
                    TempData["Success"] = "سفارش با موفقیت لغو شد.";
                else
                    TempData["Error"] = "خطا در لغو سفارش.";

                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در لغو سفارش");
                TempData["Error"] = "خطا در لغو سفارش.";
                return RedirectToAction("Detail", new { id });
            }
        }
    }
}
