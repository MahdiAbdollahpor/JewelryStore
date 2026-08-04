using JewelryStore.Services.DTOs.Admin;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(IAdminService adminService, ILogger<SettingsController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var shipping = await _adminService.GetShippingSettingsAsync();
                var tax = await _adminService.GetTaxSettingsAsync();

                var model = new SettingsViewModel
                {
                    ShippingSettings = shipping,
                    TaxSettings = tax
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تنظیمات");
                TempData["Error"] = "خطا در دریافت تنظیمات.";
                return View(new SettingsViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateShipping(UpdateShippingSettingsDto updateDto)
        {
            try
            {
                var result = await _adminService.UpdateShippingSettingsAsync(updateDto);
                if (result)
                    TempData["Success"] = "تنظیمات ارسال با موفقیت به‌روزرسانی شد.";
                else
                    TempData["Error"] = "خطا در به‌روزرسانی تنظیمات ارسال.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در به‌روزرسانی تنظیمات ارسال");
                TempData["Error"] = "خطا در به‌روزرسانی تنظیمات ارسال.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTax(UpdateTaxSettingsDto updateDto)
        {
            try
            {
                var result = await _adminService.UpdateTaxSettingsAsync(updateDto);
                if (result)
                    TempData["Success"] = "تنظیمات مالیات با موفقیت به‌روزرسانی شد.";
                else
                    TempData["Error"] = "خطا در به‌روزرسانی تنظیمات مالیات.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در به‌روزرسانی تنظیمات مالیات");
                TempData["Error"] = "خطا در به‌روزرسانی تنظیمات مالیات.";
                return RedirectToAction("Index");
            }
        }
    }

    public class SettingsViewModel
    {
        public ShippingSettingsDto ShippingSettings { get; set; } = new ShippingSettingsDto();
        public TaxSettingsDto TaxSettings { get; set; } = new TaxSettingsDto();
    }
}
