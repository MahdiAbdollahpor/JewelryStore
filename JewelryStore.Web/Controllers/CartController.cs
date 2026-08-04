using JewelryStore.Services.DTOs.Cart;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JewelryStore.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            IProductService productService,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _logger = logger;
        }

        // 1️⃣ نمایش سبد خرید
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var sessionId = GetSessionId();

            var cart = await _cartService.GetCartAsync(userId, sessionId);
            return View(cart);
        }

        // 2️⃣ افزودن به سبد خرید (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto addDto)
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();

                // بررسی موجودی قبل از افزودن
                var product = await _productService.GetProductByIdAsync(addDto.ProductId);
                if (product == null)
                    return Json(new { success = false, message = "محصول یافت نشد." });

                if (product.Quantity < addDto.Quantity)
                    return Json(new { success = false, message = $"موجودی محصول کافی نیست. موجودی: {product.Quantity}" });

                var cartItem = await _cartService.AddToCartAsync(userId, sessionId, addDto);

                var itemCount = await _cartService.GetCartItemsCountAsync(userId, sessionId);
                var total = await _cartService.GetCartTotalAsync(userId, sessionId);

                return Json(new
                {
                    success = true,
                    message = "اثر با موفقیت به کلکسیون شما افزوده شد.",
                    itemCount = itemCount,
                    total = total.ToString("N0"),
                    cartItem = cartItem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در افزودن به سبد خرید");
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // 3️⃣ به‌روزرسانی تعداد آیتم (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemDto updateDto)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return Json(new { success = false, message = "لطفاً وارد حساب کاربری خود شوید." });

                var cartItem = await _cartService.UpdateCartItemAsync(userId.Value, updateDto);

                var total = await _cartService.GetCartTotalAsync(userId, null);
                var itemCount = await _cartService.GetCartItemsCountAsync(userId, null);

                return Json(new
                {
                    success = true,
                    cartItem = cartItem,
                    total = total.ToString("N0"),
                    itemCount = itemCount,
                    message = "کلکسیون شما با موفقیت به‌روزرسانی شد."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در به‌روزرسانی تعداد");
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // 4️⃣ حذف آیتم از سبد خرید (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int id)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return Json(new { success = false, message = "لطفاً وارد حساب کاربری خود شوید." });

                var result = await _cartService.RemoveFromCartAsync(userId.Value, id);

                if (result)
                {
                    var total = await _cartService.GetCartTotalAsync(userId, null);
                    var itemCount = await _cartService.GetCartItemsCountAsync(userId, null);

                    return Json(new
                    {
                        success = true,
                        total = total.ToString("N0"),
                        itemCount = itemCount,
                        message = "اثر با موفقیت از کلکسیون شما حذف شد."
                    });
                }

                return Json(new { success = false, message = "آیتم مورد نظر یافت نشد." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف از سبد خرید");
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // 5️⃣ خالی کردن سبد خرید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            await _cartService.ClearCartAsync(userId.Value);
            TempData["Success"] = "کلکسیون شما با موفقیت خالی شد.";
            return RedirectToAction("Index");
        }

        // 6️⃣ ویجت سبد خرید (برای هدر)
        [HttpGet]
        public async Task<IActionResult> CartWidget()
        {
            var userId = GetUserId();
            var sessionId = GetSessionId();

            var itemCount = await _cartService.GetCartItemsCountAsync(userId, sessionId);
            var total = await _cartService.GetCartTotalAsync(userId, sessionId);

            return Json(new
            {
                itemCount = itemCount,
                total = total.ToString("N0")
            });
        }

        // 7️⃣ تعداد آیتم‌های سبد خرید
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var userId = GetUserId();
            var sessionId = GetSessionId();

            var count = await _cartService.GetCartItemsCountAsync(userId, sessionId);
            return Json(new { count });
        }

        // 8️⃣ تسویه حساب (هدایت به صفحه سفارش)
        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "لطفاً برای تکمیل سفارش وارد حساب کاربری خود شوید.";
                return RedirectToAction("Login", "Account");
            }

            // می‌توانیم یک ViewModel برای تسویه حساب بسازیم
            return View();
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

        private string? GetSessionId()
        {
            // بررسی وجود SessionId در کوکی
            if (Request.Cookies.TryGetValue("CartSessionId", out string? sessionId))
                return sessionId;

            // اگر وجود نداشت، یک SessionId جدید بساز
            sessionId = Guid.NewGuid().ToString();
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30), // لوکس: مدت طولانی‌تر
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            };
            Response.Cookies.Append("CartSessionId", sessionId, options);

            return sessionId;
        }
    }
}
