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

        public CartController(ICartService cartService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
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
        public async Task<IActionResult> AddToCart(AddToCartDto addDto)
        {
            try
            {
                var userId = GetUserId();
                var sessionId = GetSessionId();

                var cartItem = await _cartService.AddToCartAsync(userId, sessionId, addDto);

                // دریافت تعداد کل آیتم‌های سبد خرید
                var itemCount = await _cartService.GetCartItemsCountAsync(userId, sessionId);

                return Json(new
                {
                    success = true,
                    message = "محصول با موفقیت به سبد خرید اضافه شد.",
                    itemCount = itemCount,
                    cartItem = cartItem
                });
            }
            catch (Exception ex)
            {
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
        public async Task<IActionResult> UpdateQuantity(UpdateCartItemDto updateDto)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return Json(new { success = false, message = "لطفاً وارد حساب کاربری خود شوید." });

                var cartItem = await _cartService.UpdateCartItemAsync(userId.Value, updateDto);

                // محاسبه مجدد جمع کل
                var total = await _cartService.GetCartTotalAsync(userId, null);

                return Json(new
                {
                    success = true,
                    cartItem = cartItem,
                    total = total,
                    message = "سبد خرید با موفقیت به‌روزرسانی شد."
                });
            }
            catch (Exception ex)
            {
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
                    // محاسبه مجدد جمع کل و تعداد
                    var total = await _cartService.GetCartTotalAsync(userId, null);
                    var itemCount = await _cartService.GetCartItemsCountAsync(userId, null);

                    return Json(new
                    {
                        success = true,
                        total = total,
                        itemCount = itemCount,
                        message = "آیتم با موفقیت از سبد خرید حذف شد."
                    });
                }

                return Json(new { success = false, message = "آیتم مورد نظر یافت نشد." });
            }
            catch (Exception ex)
            {
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
                return RedirectToAction("Index", "Account");

            await _cartService.ClearCartAsync(userId.Value);
            TempData["Success"] = "سبد خرید شما خالی شد.";
            return RedirectToAction("Index");
        }

        // 6️⃣ نمایش ویجت سبد خرید (برای هدر)
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

        // 🔧 متدهای کمکی خصوصی

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
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            };
            Response.Cookies.Append("CartSessionId", sessionId, options);

            return sessionId;
        }
    }
}
