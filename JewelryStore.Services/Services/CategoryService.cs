using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Category;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JewelryStore.Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // 1 دریافت همه دسته‌ها
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(bool onlyActive = true)
        {
            var query = _context.Categories.AsQueryable();

            if (onlyActive)
                query = query.Where(c => c.IsActive);

            var categories = await query
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        // 2 دریافت دسته با شناسه
        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                throw new KeyNotFoundException("دسته‌بندی یافت نشد.");

            return _mapper.Map<CategoryDto>(category);
        }

        // 3 دریافت دسته با Slug
        public async Task<CategoryDto> GetCategoryBySlugAsync(string slug)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

            if (category == null)
                throw new KeyNotFoundException("دسته‌بندی یافت نشد.");

            return _mapper.Map<CategoryDto>(category);
        }

        // 4 دریافت زیردسته‌های یک دسته
        public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentId)
        {
            var subCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CategoryDto>>(subCategories);
        }

        // 5 دریافت درخت کامل دسته‌بندی
        public async Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync()
        {
            var allCategories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // دسته‌های ریشه (ParentCategoryId == null)
            var rootCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();

            return BuildCategoryTree(rootCategories, allCategories);
        }

        // 6 ایجاد دسته جدید (فقط ادمین)
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto)
        {
            // بررسی یکتا بودن Slug
            var slug = GenerateSlug(createDto.Name);
            if (await _context.Categories.AnyAsync(c => c.Slug == slug))
                throw new InvalidOperationException($"Slug '{slug}' قبلاً استفاده شده است.");

            var category = new Category
            {
                Name = createDto.Name,
                Slug = slug,
                ParentCategoryId = createDto.ParentCategoryId,
                IsActive = createDto.IsActive,
                ShowInMenu = createDto.ShowInMenu,
                DisplayOrder = createDto.DisplayOrder,
                CreatedAt = DateTime.Now
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        // 7 ویرایش دسته (فقط ادمین)
        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                throw new KeyNotFoundException("دسته‌بندی یافت نشد.");

            if (!string.IsNullOrWhiteSpace(updateDto.Name))
            {
                category.Name = updateDto.Name;
                if (string.IsNullOrWhiteSpace(updateDto.Slug))
                {
                    // اگر Slug جدید ارسال نشده، بر اساس Name جدید بساز
                    category.Slug = GenerateSlug(updateDto.Name);
                }
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Slug))
            {
                // بررسی یکتا بودن Slug جدید
                if (await _context.Categories.AnyAsync(c => c.Slug == updateDto.Slug && c.Id != id))
                    throw new InvalidOperationException($"Slug '{updateDto.Slug}' قبلاً استفاده شده است.");
                category.Slug = updateDto.Slug;
            }

            if (updateDto.ParentCategoryId.HasValue)
                category.ParentCategoryId = updateDto.ParentCategoryId.Value;

            if (updateDto.ShowInMenu.HasValue)
                category.ShowInMenu = updateDto.ShowInMenu.Value;

            if (updateDto.DisplayOrder.HasValue)
                category.DisplayOrder = updateDto.DisplayOrder.Value;

            category.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        // 8 حذف دسته (فقط ادمین - حذف نرم)
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                throw new KeyNotFoundException("دسته‌بندی یافت نشد.");

            // بررسی وجود زیردسته
            if (category.SubCategories.Any())
                throw new InvalidOperationException("این دسته دارای زیردسته است. ابتدا زیردسته‌ها را حذف کنید.");

            category.IsActive = false;
            category.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        // 9 تغییر وضعیت فعال/غیرفعال (فقط ادمین)
        public async Task<bool> ToggleCategoryStatusAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                throw new KeyNotFoundException("دسته‌بندی یافت نشد.");

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return category.IsActive;
        }

        // 🔧 متدهای کمکی خصوصی

        private static string GenerateSlug(string name)
        {
            // تبدیل به حروف کوچک و جایگزینی فاصله با خط تیره
            var slug = name.ToLower().Replace(" ", "-");
            // حذف کاراکترهای غیرمجاز (فقط حروف، اعداد و خط تیره)
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");
            return slug;
        }

        private IEnumerable<CategoryTreeDto> BuildCategoryTree(List<Category> rootCategories, List<Category> allCategories)
        {
            var tree = new List<CategoryTreeDto>();

            foreach (var root in rootCategories)
            {
                var node = new CategoryTreeDto
                {
                    Id = root.Id,
                    Name = root.Name,
                    Slug = root.Slug,
                    IsActive = root.IsActive,
                    ShowInMenu = root.ShowInMenu,
                    DisplayOrder = root.DisplayOrder,
                    SubCategories = BuildSubCategoryTree(root.Id, allCategories)
                };
                tree.Add(node);
            }

            return tree;
        }

        private List<CategoryTreeDto> BuildSubCategoryTree(int parentId, List<Category> allCategories)
        {
            var children = allCategories.Where(c => c.ParentCategoryId == parentId).ToList();
            var result = new List<CategoryTreeDto>();

            foreach (var child in children)
            {
                var node = new CategoryTreeDto
                {
                    Id = child.Id,
                    Name = child.Name,
                    Slug = child.Slug,
                    IsActive = child.IsActive,
                    ShowInMenu = child.ShowInMenu,
                    DisplayOrder = child.DisplayOrder,
                    SubCategories = BuildSubCategoryTree(child.Id, allCategories)
                };
                result.Add(node);
            }

            return result;
        }
    }
}
