using JewelryStore.Services.DTOs.Category;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AttributesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<AttributesController> _logger;

        public AttributesController(ICategoryService categoryService, ILogger<AttributesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // ==================== لیست ویژگی‌ها ====================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var attributes = await _categoryService.GetAllAttributesAsync();
                return View(attributes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست ویژگی‌ها");
                TempData["Error"] = "خطا در دریافت لیست ویژگی‌ها.";
                return View(new List<CategoryAttributeDto>());
            }
        }

        // ==================== ایجاد ویژگی جدید ====================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                return View(new CreateCategoryAttributeDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت دسته‌بندی‌ها");
                TempData["Error"] = "خطا در دریافت دسته‌بندی‌ها.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryAttributeDto createDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                return View(createDto);
            }

            try
            {
                await _categoryService.CreateAttributeAsync(createDto);
                TempData["Success"] = "ویژگی با موفقیت ایجاد شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد ویژگی");
                ModelState.AddModelError("", ex.Message);
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                return View(createDto);
            }
        }

        // ==================== ویرایش ویژگی ====================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var attribute = await _categoryService.GetAttributeByIdAsync(id);
                if (attribute == null)
                    return NotFound();

                // ✅ ارسال لیست دسته‌بندی‌ها به View
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);

                // ✅ مدل شامل Type و IsRequired است
                return View(attribute);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت ویژگی");
                TempData["Error"] = "خطا در دریافت ویژگی.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCategoryAttributeDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                return View(updateDto);
            }

            try
            {
                await _categoryService.UpdateAttributeAsync(id, updateDto);
                TempData["Success"] = "ویژگی با موفقیت ویرایش شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ویرایش ویژگی");
                ModelState.AddModelError("", ex.Message);
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                return View(updateDto);
            }
        }

        // ==================== حذف ویژگی ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _categoryService.DeleteAttributeAsync(id);
                if (result)
                    TempData["Success"] = "ویژگی با موفقیت حذف شد.";
                else
                    TempData["Error"] = "خطا در حذف ویژگی.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف ویژگی");
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        // ==================== تغییر وضعیت ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                // این متد را باید به ICategoryService اضافه کنید
                // await _categoryService.ToggleAttributeStatusAsync(id);
                TempData["Success"] = "وضعیت ویژگی با موفقیت تغییر کرد.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت ویژگی");
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
