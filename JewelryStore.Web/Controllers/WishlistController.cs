using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JewelryStore.Web.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
        {
            _wishlistService = wishlistService;
            _logger = logger;
        }

        // 1️⃣ نمایش لیست علاقه‌مندی‌ها
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var wishlist = await _wishlistService.GetUserWishlistAsync(userId.Value);
            return View(wishlist);
        }

        // 2️⃣ افزودن به علاقه‌مندی‌ها (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return Json(new { success = false, message = "لطفاً وارد حساب کاربری خود شوید." });

                var result = await _wishlistService.AddToWishlistAsync(userId.Value, productId);

                if (result)
                {
                    var count = await _wishlistService.GetWishlistCountAsync(userId.Value);
                    return Json(new
                    {
                        success = true,
                        message = "اثر با موفقیت به لیست علاقه‌مندی‌ها افزوده شد.",
                        count = count,
                        isInWishlist = true
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "این اثر قبلاً به لیست علاقه‌مندی‌ها اضافه شده است."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در افزودن به علاقه‌مندی‌ها");
                return Json(new
                {
                    success = false,
                    message = "خطا در افزودن به لیست علاقه‌مندی‌ها."
                });
            }
        }

        // 3️⃣ حذف از علاقه‌مندی‌ها (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return Json(new { success = false, message = "لطفاً وارد حساب کاربری خود شوید." });

                var result = await _wishlistService.RemoveFromWishlistAsync(userId.Value, productId);

                if (result)
                {
                    var count = await _wishlistService.GetWishlistCountAsync(userId.Value);
                    return Json(new
                    {
                        success = true,
                        message = "اثر با موفقیت از لیست علاقه‌مندی‌ها حذف شد.",
                        count = count,
                        isInWishlist = false
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "اثر در لیست علاقه‌مندی‌ها یافت نشد."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف از علاقه‌مندی‌ها");
                return Json(new
                {
                    success = false,
                    message = "خطا در حذف از لیست علاقه‌مندی‌ها."
                });
            }
        }

        // 4️⃣ بررسی وجود محصول در علاقه‌مندی‌ها (AJAX)
        [HttpGet]
        public async Task<IActionResult> IsInWishlist(int productId)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return Json(new { isInWishlist = false });

                var result = await _wishlistService.IsInWishlistAsync(userId.Value, productId);
                return Json(new { isInWishlist = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی علاقه‌مندی");
                return Json(new { isInWishlist = false });
            }
        }

        // 5️⃣ تعداد علاقه‌مندی‌ها (برای ویجت)
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return Json(new { count = 0 });

                var count = await _wishlistService.GetWishlistCountAsync(userId.Value);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تعداد علاقه‌مندی‌ها");
                return Json(new { count = 0 });
            }
        }

        // 6️⃣ خالی کردن لیست علاقه‌مندی‌ها
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return RedirectToAction("Login", "Account");

                await _wishlistService.ClearWishlistAsync(userId.Value);
                TempData["Success"] = "لیست علاقه‌مندی‌ها با موفقیت خالی شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در خالی کردن لیست علاقه‌مندی‌ها");
                TempData["Error"] = "خطا در خالی کردن لیست علاقه‌مندی‌ها.";
                return RedirectToAction("Index");
            }
        }

        // 7️⃣ تغییر وضعیت علاقه‌مندی (افزودن/حذف) - برای دکمه‌های Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int productId)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return Json(new { success = false, message = "لطفاً وارد حساب کاربری خود شوید." });

                var isInWishlist = await _wishlistService.IsInWishlistAsync(userId.Value, productId);

                if (isInWishlist)
                {
                    await _wishlistService.RemoveFromWishlistAsync(userId.Value, productId);
                }
                else
                {
                    await _wishlistService.AddToWishlistAsync(userId.Value, productId);
                }

                var count = await _wishlistService.GetWishlistCountAsync(userId.Value);

                return Json(new
                {
                    success = true,
                    isInWishlist = !isInWishlist,
                    count = count,
                    message = isInWishlist ? "از علاقه‌مندی‌ها حذف شد" : "به علاقه‌مندی‌ها افزوده شد"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت علاقه‌مندی");
                return Json(new
                {
                    success = false,
                    message = "خطا در تغییر وضعیت علاقه‌مندی."
                });
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