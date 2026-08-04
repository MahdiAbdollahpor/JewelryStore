using JewelryStore.Services.DTOs.User;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAdminService adminService, ILogger<UsersController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(UserFilterDto filter)
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync(filter);
                ViewBag.CurrentFilter = filter;
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست کاربران");
                TempData["Error"] = "خطا در دریافت لیست کاربران.";
                return View(new List<UserListDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var user = await _adminService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound();

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت جزئیات کاربر");
                TempData["Error"] = "خطا در دریافت جزئیات کاربر.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var result = await _adminService.ToggleUserStatusAsync(id);
                if (result)
                    TempData["Success"] = "وضعیت کاربر با موفقیت تغییر کرد.";
                else
                    TempData["Error"] = "خطا در تغییر وضعیت کاربر.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت کاربر");
                TempData["Error"] = "خطا در تغییر وضعیت کاربر.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            try
            {
                var result = await _adminService.ChangeUserRoleAsync(id, role);
                if (result)
                    TempData["Success"] = "نقش کاربر با موفقیت تغییر کرد.";
                else
                    TempData["Error"] = "خطا در تغییر نقش کاربر.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر نقش کاربر");
                TempData["Error"] = "خطا در تغییر نقش کاربر.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _adminService.DeleteUserAsync(id);
                if (result)
                    TempData["Success"] = "کاربر با موفقیت حذف شد.";
                else
                    TempData["Error"] = "خطا در حذف کاربر.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف کاربر");
                TempData["Error"] = "خطا در حذف کاربر.";
                return RedirectToAction("Index");
            }
        }
    }
}
