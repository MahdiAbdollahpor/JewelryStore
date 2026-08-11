using JewelryStore.Services.DTOs.Product;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // 1️⃣ لیست محصولات با فیلتر و صفحه‌بندی
        [HttpGet]
        public async Task<IActionResult> Index(ProductFilterDto filter)
        {
            var (products, totalCount) = await _productService.GetProductsAsync(filter);

            // دریافت لیست دسته‌بندی‌ها برای فیلتر (با استفاده از ICategoryService)
            var categories = await _categoryService.GetAllCategoriesAsync(true);
            ViewBag.Categories = categories;
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentFilter = filter;

            return View(products);
        }

        // 2️⃣ جزئیات محصول با Slug
        [HttpGet]
        public async Task<IActionResult> Details(string slug)
        {
            try
            {
                var product = await _productService.GetProductBySlugAsync(slug);

                // ✅ تصاویر قبلاً در سرویس تنظیم شده‌اند
                // فقط اگر تصویر اصلی وجود نداشت، تصویر پیش‌فرض نمایش داده شود
                if (string.IsNullOrEmpty(product.MainImageUrl))
                {
                    product.MainImageUrl = "/images/no-image.png";
                }

                // دریافت محصولات مرتبط (هم‌دسته)
                var relatedProducts = await _productService.GetRelatedProductsAsync(product.Id, 4);
                ViewBag.RelatedProducts = relatedProducts;

                return View(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // 3️⃣ جستجوی محصولات
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return RedirectToAction("Index");

            var products = await _productService.SearchProductsAsync(term);
            ViewBag.SearchTerm = term;
            return View(products);
        }
    }
}
