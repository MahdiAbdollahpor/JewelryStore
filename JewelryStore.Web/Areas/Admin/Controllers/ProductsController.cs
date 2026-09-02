using JewelryStore.Services.DTOs.Admin;
using JewelryStore.Services.DTOs.Product;
using JewelryStore.Services.Interfaces;
using JewelryStore.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Areas.Admin.Controllers
{
  [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IAdminService adminService,
            IProductService productService,
            ILogger<ProductsController> logger,
            ICategoryService categoryService)
        {
            _adminService = adminService;
            _productService = productService;
            _logger = logger;
            _categoryService = categoryService;
        }

        // ✅ لیست محصولات
        [HttpGet]
        public async Task<IActionResult> Index(AdminProductFilterDto filter)
        {
            // ✅ تنظیم مقادیر پیش‌فرض
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10; // 10 محصول در هر صفحه

            try
            {
                var products = await _adminService.GetAllProductsAsync(filter);

                // ✅ دریافت تعداد کل محصولات برای صفحه‌بندی
                var totalCount = await _productService.GetTotalProductsCountAsync(filter);

                ViewBag.CurrentFilter = filter;
                ViewBag.TotalCount = totalCount;
                ViewBag.CurrentPage = filter.Page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);
                ViewBag.PageSize = filter.PageSize;

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست محصولات");
                TempData["Error"] = "خطا در دریافت لیست محصولات.";
                return View(new List<ProductListDto>());
            }
        }

        // ✅ جزئیات محصول
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var product = await _adminService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت جزئیات محصول");
                TempData["Error"] = "خطا در دریافت جزئیات محصول.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // ✅ دریافت لیست دسته‌بندی‌ها برای نمایش در Dropdown
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(true);

            var attributes = await _categoryService.GetAllAttributesAsync();
            ViewBag.Attributes = attributes;

            return View(new CreateProductDto());
        }

        // ✅ ذخیره محصول جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto createDto, List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(true);
                ViewBag.Attributes = await _categoryService.GetAllAttributesAsync();
                return View(createDto);
            }

            try
            {
                createDto.ImageFiles = images;
                var product = await _adminService.CreateProductAsync(createDto);
                TempData["Success"] = "محصول با موفقیت ایجاد شد.";
                return RedirectToAction("Detail", new { id = product.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد محصول");
                ModelState.AddModelError("", ex.Message);
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(true);
                ViewBag.Attributes = await _categoryService.GetAllAttributesAsync();
                return View(createDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _adminService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound();

                // ✅ دریافت لیست دسته‌بندی‌ها برای نمایش در Dropdown
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(true);
                ViewBag.ProductId = id;

                var updateDto = new UpdateProductDto
                {
                    Name = product.Name,
                    Slug = product.Slug,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand,
                    Description = product.Description,
                    ShortDescription = product.ShortDescription,
                    BasePrice = product.BasePrice,
                    DiscountPercentage = product.DiscountPercentage,
                    Weight = product.Weight,
                    Purity = product.Purity,
                    CraftsmanshipFee = product.CraftsmanshipFee,
                    StoneType = product.StoneType,
                    StoneWeight = product.StoneWeight,
                    StoneQuality = product.StoneQuality,
                    Quantity = product.Quantity,
                    MinOrderQuantity = product.MinOrderQuantity,
                    MaxOrderQuantity = product.MaxOrderQuantity,
                    IsActive = product.IsActive,
                    IsFeatured = product.IsFeatured,
                    IsNew = product.IsNew,
                    Tags = product.Tags
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اطلاعات محصول");
                TempData["Error"] = "خطا در دریافت اطلاعات محصول.";
                return RedirectToAction("Index");
            }
        }

        // ✅ ذخیره ویرایش محصول
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductDto updateDto, List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductId = id;
                return View(updateDto);
            }

            try
            {
                updateDto.ImageFiles = images;
                await _adminService.UpdateProductAsync(id, updateDto);
                TempData["Success"] = "محصول با موفقیت ویرایش شد.";
                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ویرایش محصول");
                ModelState.AddModelError("", ex.Message);
                ViewBag.ProductId = id;
                return View(updateDto);
            }
        }

        // ✅ تغییر وضعیت محصول
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var result = await _adminService.ToggleProductStatusAsync(id);
                if (result)
                    TempData["Success"] = "وضعیت محصول با موفقیت تغییر کرد.";
                else
                    TempData["Error"] = "خطا در تغییر وضعیت محصول.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر وضعیت محصول");
                TempData["Error"] = "خطا در تغییر وضعیت محصول.";
                return RedirectToAction("Index");
            }
        }

        // ✅ حذف محصول
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _adminService.DeleteProductAsync(id);
                if (result)
                    TempData["Success"] = "محصول با موفقیت حذف شد.";
                else
                    TempData["Error"] = "خطا در حذف محصول.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف محصول");
                TempData["Error"] = "خطا در حذف محصول.";
                return RedirectToAction("Index");
            }
        }
    }
}
