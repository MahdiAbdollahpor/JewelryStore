using JewelryStore.Services.DTOs.User;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JewelryStore.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // ==================== ثبت‌نام ====================

        // 1️⃣ صفحه ثبت‌نام
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new RegisterDto());
        }

        // 2️⃣ ثبت‌نام جدید (ارسال کد تایید)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return View(registerDto);

            try
            {
                var result = await _userService.RegisterAsync(registerDto);

                if (result.IsSuccess)
                {
                    TempData["PhoneNumber"] = result.PhoneNumber;
                    TempData["Success"] = result.Message;
                    return RedirectToAction("VerifyPhone");
                }

                ModelState.AddModelError("", result.Message);
                return View(registerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت‌نام");
                ModelState.AddModelError("", "خطا در ثبت‌نام. لطفاً مجدداً تلاش کنید.");
                return View(registerDto);
            }
        }

        // ==================== تایید شماره موبایل ====================

        // 3️⃣ صفحه تایید شماره موبایل
        [HttpGet]
        public IActionResult VerifyPhone()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            var phoneNumber = TempData["PhoneNumber"]?.ToString();
            if (string.IsNullOrEmpty(phoneNumber))
                return RedirectToAction("Register");

            return View(new VerifyPhoneDto { PhoneNumber = phoneNumber });
        }

        // 4️⃣ تایید شماره موبایل با کد
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhone(VerifyPhoneDto verifyDto)
        {
            if (!ModelState.IsValid)
                return View(verifyDto);

            try
            {
                var result = await _userService.VerifyPhoneAsync(verifyDto);

                if (result.IsSuccess)
                {
                    // ورود خودکار کاربر
                    await SignInUser(result.User.Id);
                    TempData["Success"] = "شماره موبایل با موفقیت تایید شد. خوش آمدید!";
                    return RedirectToAction("Index", "Product");
                }

                if (result.CodeExpired)
                {
                    ModelState.AddModelError("", "کد تایید منقضی شده است. لطفاً کد جدید درخواست کنید.");
                    return View(verifyDto);
                }

                ModelState.AddModelError("", result.Message);
                return View(verifyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تایید شماره موبایل");
                ModelState.AddModelError("", "خطا در تایید شماره موبایل. لطفاً مجدداً تلاش کنید.");
                return View(verifyDto);
            }
        }

        // 5️⃣ ارسال مجدد کد تایید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return BadRequest("شماره موبایل مشخص نشده است.");

            try
            {
                var result = await _userService.ResendVerificationCodeAsync(phoneNumber);
                if (result)
                {
                    TempData["Success"] = "کد تایید مجدداً به شماره شما ارسال شد.";
                    return RedirectToAction("VerifyPhone");
                }

                TempData["Error"] = "خطا در ارسال مجدد کد تایید.";
                return RedirectToAction("Register");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال مجدد کد تایید");
                TempData["Error"] = "خطا در ارسال مجدد کد تایید.";
                return RedirectToAction("Register");
            }
        }

        // ==================== ورود ====================

        // 6️⃣ صفحه ورود
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginDto());
        }

        // 7️⃣ ورود با رمز عبور
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto loginDto, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(loginDto);
            }

            try
            {
                var result = await _userService.LoginAsync(loginDto);

                if (result.IsSuccess && result.User != null)
                {
                    await SignInUser(result.User.Id);
                    TempData["Success"] = "خوش آمدید!";

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Product");
                }

                ModelState.AddModelError("", result.Message);
                ViewBag.ReturnUrl = returnUrl;
                return View(loginDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ورود");
                ModelState.AddModelError("", "خطا در ورود. لطفاً مجدداً تلاش کنید.");
                ViewBag.ReturnUrl = returnUrl;
                return View(loginDto);
            }
        }

        // 8️⃣ صفحه ورود با کد یکبارمصرف
        [HttpGet]
        public IActionResult LoginWithCode(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // 9️⃣ درخواست کد ورود یکبارمصرف
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLoginCode(string phoneNumber, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                TempData["Error"] = "لطفاً شماره موبایل خود را وارد کنید.";
                return RedirectToAction("LoginWithCode");
            }

            try
            {
                // ارسال کد تایید به شماره کاربر
                var result = await _userService.ResendVerificationCodeAsync(phoneNumber);
                if (result)
                {
                    TempData["PhoneNumber"] = phoneNumber;
                    TempData["Success"] = "کد ورود به شماره شما ارسال شد.";
                    return RedirectToAction("VerifyLoginCode", new { returnUrl });
                }

                TempData["Error"] = "کاربری با این شماره موبایل یافت نشد.";
                return RedirectToAction("LoginWithCode");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال کد ورود");
                TempData["Error"] = "خطا در ارسال کد ورود.";
                return RedirectToAction("LoginWithCode");
            }
        }

        // 🔟 صفحه تایید کد ورود
        [HttpGet]
        public IActionResult VerifyLoginCode(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            var phoneNumber = TempData["PhoneNumber"]?.ToString();
            if (string.IsNullOrEmpty(phoneNumber))
                return RedirectToAction("LoginWithCode");

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.PhoneNumber = phoneNumber;
            return View();
        }

        // 1️⃣1️⃣ تایید کد ورود یکبارمصرف
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyLoginCode(string phoneNumber, string code, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(code))
            {
                TempData["Error"] = "اطلاعات کامل نیست.";
                return RedirectToAction("LoginWithCode");
            }

            try
            {
                var result = await _userService.LoginWithCodeAsync(phoneNumber, code);

                if (result.IsSuccess && result.User != null)
                {
                    await SignInUser(result.User.Id);
                    TempData["Success"] = "خوش آمدید!";

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Product");
                }

                TempData["Error"] = result.Message;
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.PhoneNumber = phoneNumber;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تایید کد ورود");
                TempData["Error"] = "خطا در تایید کد ورود.";
                return RedirectToAction("LoginWithCode");
            }
        }

        // ==================== پروفایل کاربر ====================

        // 1️⃣2️⃣ نمایش پروفایل کاربر
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login");

            try
            {
                var user = await _userService.GetProfileAsync(userId.Value);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در نمایش پروفایل");
                TempData["Error"] = "خطا در نمایش پروفایل.";
                return RedirectToAction("Index", "Product");
            }
        }

        // 1️⃣3️⃣ ویرایش پروفایل
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateDto)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                var user = await _userService.GetProfileAsync(userId.Value);
                return View("Profile", user);
            }

            try
            {
                await _userService.UpdateProfileAsync(userId.Value, updateDto);
                TempData["Success"] = "اطلاعات با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در به‌روزرسانی پروفایل");
                TempData["Error"] = "خطا در به‌روزرسانی پروفایل.";
                return RedirectToAction("Profile");
            }
        }

        // 1️⃣4️⃣ تغییر رمز عبور
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "اطلاعات وارد شده معتبر نیست.";
                return RedirectToAction("Profile");
            }

            try
            {
                await _userService.ChangePasswordAsync(userId.Value, changePasswordDto);
                TempData["Success"] = "رمز عبور با موفقیت تغییر کرد.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر رمز عبور");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Profile");
            }
        }

        // ==================== خروج ====================

        // 1️⃣5️⃣ خروج از حساب کاربری
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "با موفقیت خارج شدید.";
            return RedirectToAction("Index", "Product");
        }

        // ==================== متدهای کمکی ====================

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

        private async Task SignInUser(int userId)
        {
            // دریافت نقش از دیتابیس
            var user = await _userService.GetProfileAsync(userId);
            if (user == null)
                throw new InvalidOperationException("کاربر یافت نشد.");

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role) // نقش از دیتابیس
    };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}
