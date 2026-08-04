using JewelryStore.Services.DTOs.Admin;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DiscountsController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<DiscountsController> _logger;

        public DiscountsController(IAdminService adminService, ILogger<DiscountsController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AdminDiscountFilterDto filter)
        {
            try
            {
                var discounts = await _adminService.GetAllDiscountsAsync(filter);
                ViewBag.CurrentFilter = filter;
                return View(discounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست تخفیف‌ها");
                TempData["Error"] = "خطا در دریافت لیست تخفیف‌ها.";
                return View(new List<DiscountListDto>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateDiscountDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDiscountDto createDto)
        {
            if (!ModelState.IsValid)
                return View(createDto);

            try
            {
                var discount = await _adminService.CreateDiscountAsync(createDto);
                TempData["Success"] = "کد تخفیف با موفقیت ایجاد شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد کد تخفیف");
                ModelState.AddModelError("", ex.Message);
                return View(createDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var result = await _adminService.ToggleDiscountStatusAsync(id);
                if (result)
                    TempData["Success"] = "وضعیت کد تخفیف با موفقیت تغییر کرد.";
                else
                    TempData["Error"] = "خطا در تغییر وضعیت کد تخفیف.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت کد تخفیف");
                TempData["Error"] = "خطا در تغییر وضعیت کد تخفیف.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _adminService.DeleteDiscountAsync(id);
                if (result)
                    TempData["Success"] = "کد تخفیف با موفقیت حذف شد.";
                else
                    TempData["Error"] = "خطا در حذف کد تخفیف.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف کد تخفیف");
                TempData["Error"] = "خطا در حذف کد تخفیف.";
                return RedirectToAction("Index");
            }
        }
    }
}
