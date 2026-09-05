using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.Admin;
using JewelryStore.Services.Interfaces;
using JewelryStore.Services.Services;
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
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public OrdersController(IAdminService adminService, ILogger<OrdersController> logger, IOrderService orderService, IPaymentService paymentService)
        {
            _adminService = adminService;
            _logger = logger;
            _orderService = orderService;
            _paymentService = paymentService;
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
                if (string.IsNullOrWhiteSpace(reason))
                {
                    TempData["Error"] = "لطفاً دلیل لغو سفارش را وارد کنید.";
                    return RedirectToAction("Detail", new { id });
                }

                // 1️⃣ دریافت سفارش
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound("سفارش یافت نشد.");

                // 2️⃣ بررسی وضعیت سفارش
                if (order.OrderStatus == OrderStatus.Delivered || order.OrderStatus == OrderStatus.Cancelled)
                {
                    TempData["Error"] = "این سفارش قابل لغو نیست.";
                    return RedirectToAction("Detail", new { id });
                }

                // 3️⃣ اگر سفارش پرداخت شده باشد، برگشت وجه انجام شود
                if (order.PaymentStatus == PaymentStatus.Paid && order.PaymentMethod == PaymentMethod.Online)
                {
                    try
                    {
                        // ✅ بررسی اینکه PaymentReference وجود دارد
                        var reference = !string.IsNullOrEmpty(order.PaymentReference)
                            ? order.PaymentReference
                            : order.OrderNumber;

                        // ✅ بررسی اینکه _paymentService null نیست
                        if (_paymentService == null)
                        {
                            TempData["Error"] = "سرویس پرداخت در دسترس نیست.";
                            return RedirectToAction("Detail", new { id });
                        }

                        var refundResult = await _paymentService.RefundPaymentAsync(
                            reference,
                            (int)order.TotalAmount
                        );

                        if (!refundResult.IsSuccess)
                        {
                            TempData["Error"] = $"خطا در برگشت وجه: {refundResult.Message}. لطفاً به صورت دستی اقدام کنید.";
                            return RedirectToAction("Detail", new { id });
                        }

                        await _orderService.UpdatePaymentStatusAsync(id, PaymentStatus.Refunded);
                        TempData["Success"] = "وجه سفارش با موفقیت به کاربر برگشت داده شد.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "خطا در برگشت وجه");
                        TempData["Error"] = $"خطا در برگشت وجه: {ex.Message}";
                        return RedirectToAction("Detail", new { id });
                    }
                }

                // 4️⃣ لغو سفارش
                await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Cancelled, $"لغو توسط ادمین: {reason}");

                TempData["Success"] = order.PaymentStatus == PaymentStatus.Paid
                    ? "سفارش با موفقیت لغو شد و وجه به کاربر برگشت داده شد."
                    : "سفارش با موفقیت لغو شد.";

                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در لغو سفارش");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Detail", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(int id, string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    TempData["Error"] = "لطفاً دلیل برگشت وجه را وارد کنید.";
                    return RedirectToAction("Detail", new { id });
                }

                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound("سفارش یافت نشد.");

                if (order.PaymentStatus != PaymentStatus.Paid)
                {
                    TempData["Error"] = "این سفارش پرداخت نشده است.";
                    return RedirectToAction("Detail", new { id });
                }

                if (order.PaymentMethod != PaymentMethod.Online)
                {
                    TempData["Error"] = "برگشت وجه فقط برای پرداخت‌های آنلاین امکان‌پذیر است.";
                    return RedirectToAction("Detail", new { id });
                }

                var reference = !string.IsNullOrEmpty(order.PaymentReference)
                    ? order.PaymentReference
                    : order.OrderNumber;

                var refundResult = await _paymentService.RefundPaymentAsync(
                    reference,
                    (int)order.TotalAmount
                );

                if (!refundResult.IsSuccess)
                {
                    TempData["Error"] = $"خطا در برگشت وجه: {refundResult.Message}";
                    return RedirectToAction("Detail", new { id });
                }

                await _orderService.UpdatePaymentStatusAsync(id, PaymentStatus.Refunded);
                await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Cancelled, $"برگشت وجه توسط ادمین: {reason}");

                TempData["Success"] = "برگشت وجه با موفقیت انجام شد.";
                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در برگشت وجه");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Detail", new { id });
            }
        }
    }
}
