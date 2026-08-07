using JewelryStore.Services.DTOs.Category;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync(false);
            return View(categories);
        }

        // ✅ اصلاح: ارسال ViewBag.Categories به Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
            return View(new CreateCategoryDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryDto createDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                return View(createDto);
            }

            try
            {
                await _categoryService.CreateCategoryAsync(createDto);
                TempData["Success"] = "دسته‌بندی با موفقیت ایجاد شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد دسته‌بندی");
                ModelState.AddModelError("", ex.Message);
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                return View(createDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound();

                var updateDto = new UpdateCategoryDto
                {
                    Name = category.Name,
                    Slug = category.Slug,
                    ParentCategoryId = category.ParentCategoryId,
                    ShowInMenu = category.ShowInMenu,
                    DisplayOrder = category.DisplayOrder,
                    IsActive = category.IsActive // ✅ اضافه کنید
                };

                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                ViewBag.CategoryId = id; // برای جلوگیری از انتخاب خودش به عنوان والد
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اطلاعات دسته‌بندی");
                TempData["Error"] = "خطا در دریافت اطلاعات دسته‌بندی.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCategoryDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                ViewBag.CategoryId = id;
                return View(updateDto);
            }

            try
            {
                await _categoryService.UpdateCategoryAsync(id, updateDto);
                TempData["Success"] = "دسته‌بندی با موفقیت ویرایش شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ویرایش دسته‌بندی");
                ModelState.AddModelError("", ex.Message);
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false);
                ViewBag.CategoryId = id;
                return View(updateDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                TempData["Success"] = "دسته‌بندی با موفقیت حذف شد.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف دسته‌بندی");
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                await _categoryService.ToggleCategoryStatusAsync(id);
                TempData["Success"] = "وضعیت دسته‌بندی با موفقیت تغییر کرد.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت دسته‌بندی");
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
