using JewelryStore.Domain.Enums;
using JewelryStore.Infrastructure.Services.Payment;
using JewelryStore.Services.DTOs.Order;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JewelryStore.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<OrderController> _logger;
        private readonly IDiscountService _discountService;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            IPaymentService paymentService,
            ILogger<OrderController> logger,
            IDiscountService discountService)
        {
            _orderService = orderService;
            _cartService = cartService;
            _paymentService = paymentService;
            _logger = logger;
            _discountService = discountService;
        }



        // 2️⃣ ثبت سفارش و هدایت به درگاه پرداخت
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(CreateOrderDto createDto)
        {
            // ✅ اگر مدل نامعتبر است (آدرس خالی و ...)
            if (!ModelState.IsValid)
            {
                // ✅ خطاها را به TempData اضافه کن (اختیاری)
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = string.Join(" - ", errors);

                // ✅ به صفحه Checkout در کنترلر Cart برگرد
                return RedirectToAction("Checkout", "Cart");
            }

            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    TempData["Error"] = "لطفاً وارد حساب کاربری خود شوید.";
                    return RedirectToAction("Login", "Account");
                }

                createDto.UserId = userId.Value;

                // دریافت کد تخفیف از TempData
                if (TempData["DiscountCode"] != null)
                {
                    createDto.DiscountCode = TempData["DiscountCode"].ToString();
                }

                var result = await _orderService.CreateOrderAsync(createDto);

                if (result != null && result.OrderId > 0)
                {
                    return RedirectToAction("Pay", new { orderId = result.OrderId });
                }

                TempData["Error"] = "خطا در ثبت سفارش.";
                return RedirectToAction("Checkout", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت سفارش");
                TempData["Error"] = $"خطا در ثبت سفارش: {ex.Message}";
                return RedirectToAction("Checkout", "Cart");
            }
        }

        // 3️⃣ پرداخت سفارش (اتصال به درگاه)
        [HttpGet]
        public async Task<IActionResult> Pay(int orderId)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return RedirectToAction("Login", "Account");

                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound("سفارش یافت نشد.");

                // TODO: اتصال به درگاه زرین‌پال
                // برای تست، یک لینک ساختگی ایجاد می‌کنیم
                var paymentResult = new PaymentResult
                {
                    IsSuccess = true,
                    PaymentUrl = "/Order/PaymentCallback?status=success&orderId=" + orderId,
                    Authority = Guid.NewGuid().ToString()
                };

                ViewBag.Order = order;
                ViewBag.PaymentUrl = paymentResult.PaymentUrl;

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پرداخت");
                TempData["Error"] = "خطا در اتصال به درگاه پرداخت.";
                return RedirectToAction("History");
            }
        }

        // 4️⃣ بازگشت از درگاه پرداخت (کالبک)
        [HttpGet]
        public async Task<IActionResult> PaymentCallback(string status, string authority, int orderId)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return RedirectToAction("Login", "Account");

                // تأیید پرداخت
                if (status == "success")
                {
                    // دریافت سفارش برای محاسبه مبلغ
                    var order = await _orderService.GetOrderByIdAsync(orderId);
                    if (order == null)
                        return NotFound("سفارش یافت نشد.");

                    // اگر مبلغ 0 باشد، پرداخت موفق
                    if (order.TotalAmount <= 0)
                    {
                        await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Paid, "پرداخت با موفقیت انجام شد.");
                        TempData["Success"] = "سفارش با موفقیت ثبت و پرداخت شد.";
                        return RedirectToAction("Details", new { orderId });
                    }

                    // TODO: تایید پرداخت با زرین‌پال
                    // var verifyResult = await _paymentService.VerifyPaymentAsync(authority, (int)order.TotalAmount);

                    // برای تست، پرداخت موفق فرض می‌شود
                    await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Paid, "پرداخت با موفقیت انجام شد.");
                    TempData["Success"] = "سفارش با موفقیت ثبت و پرداخت شد.";
                    return RedirectToAction("Details", new { orderId });
                }

                TempData["Error"] = "پرداخت ناموفق بود. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Pay", new { orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در کالبک پرداخت");
                TempData["Error"] = "خطا در فرآیند پرداخت.";
                return RedirectToAction("History");
            }
        }

        // 5️⃣ تاریخچه سفارشات کاربر
        [HttpGet]
        public async Task<IActionResult> History(int page = 1, int pageSize = 10)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var orders = await _orderService.GetUserOrdersAsync(userId.Value, page, pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalOrders = orders.Count();

            return View(orders);
        }

        // 6️⃣ جزئیات یک سفارش
        [HttpGet]
        public async Task<IActionResult> Details(int orderId)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return RedirectToAction("Login", "Account");

                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound("سفارش یافت نشد.");

                // بررسی دسترسی: فقط خود کاربر یا ادمین
                // TODO: اضافه کردن چک ادمین

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در نمایش جزئیات سفارش");
                TempData["Error"] = "خطا در نمایش جزئیات سفارش.";
                return RedirectToAction("History");
            }
        }

       




        //// 7️⃣ لغو سفارش (فقط قبل از پرداخت)
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CancelOrder(int orderId)
        //{
        //    try
        //    {
        //        var userId = GetUserId();
        //        if (!userId.HasValue)
        //            return RedirectToAction("Login", "Account");

        //        var order = await _orderService.GetOrderByIdAsync(orderId);
        //        if (order == null)
        //            return NotFound("سفارش یافت نشد.");

        //        // ✅ کاربر فقط می‌تواند سفارش‌های در انتظار پرداخت را لغو کند
        //        if (order.OrderStatus != OrderStatus.Pending)
        //        {
        //            TempData["Error"] = "امکان لغو این سفارش وجود ندارد. سفارشات پرداخت‌شده قابل لغو نیستند.";
        //            return RedirectToAction("Details", new { orderId });
        //        }

        //        await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled, "لغو توسط کاربر");
        //        TempData["Success"] = "سفارش با موفقیت لغو شد.";
        //        return RedirectToAction("History");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "خطا در لغو سفارش");
        //        TempData["Error"] = "خطا در لغو سفارش.";
        //        return RedirectToAction("Details", new { orderId });
        //    }
        //}

        // 8️⃣ دریافت کد رهگیری (برای کاربر)
        [HttpGet]
        public async Task<IActionResult> Tracking(int orderId)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return RedirectToAction("Login", "Account");

                // دریافت سفارش از سرویس
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound("سفارش یافت نشد.");

                // ✅ دریافت UserId از دیتابیس با یک کوئری جداگانه
                var orderEntity = await _orderService.GetOrderEntityByIdAsync(orderId);
                if (orderEntity == null || orderEntity.UserId != userId.Value)
                    return NotFound("سفارش یافت نشد.");

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در نمایش کد رهگیری");
                TempData["Error"] = "خطا در نمایش کد رهگیری.";
                return RedirectToAction("History");
            }
        }

        // 🔧 متدهای کمکی


        private int? GetUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    return userId;
            }
            return null;
        }
    }
}