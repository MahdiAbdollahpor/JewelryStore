using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Category;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

        // 6️⃣ ایجاد دسته جدید (فقط ادمین)
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto)
        {
            // اگر Slug ارسال نشده، از Name تولید کن
            var slug = string.IsNullOrWhiteSpace(createDto.Slug)
                ? GenerateSlug(createDto.Name)
                : createDto.Slug;

            // بررسی یکتا بودن Slug
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

        public async Task<IEnumerable<CategoryAttributeDto>> GetAllAttributesAsync()
        {
            var attributes = await _context.CategoryAttributes
                .Include(a => a.Category)
                .OrderBy(a => a.Category.Name)
                .ThenBy(a => a.Name)
                .ToListAsync();

            // ✅ اگر هیچ ویژگی وجود ندارد، یک لیست خالی برگردان
            if (attributes == null || !attributes.Any())
            {
                return new List<CategoryAttributeDto>();
            }

            return _mapper.Map<IEnumerable<CategoryAttributeDto>>(attributes);
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

        // ==================== مدیریت ویژگی‌ها ====================

        

        public async Task<CategoryAttributeDto> GetAttributeByIdAsync(int id)
        {
            var attribute = await _context.CategoryAttributes
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attribute == null)
                throw new KeyNotFoundException("ویژگی یافت نشد.");

            return _mapper.Map<CategoryAttributeDto>(attribute);
        }

        public async Task<CategoryAttributeDto> CreateAttributeAsync(CreateCategoryAttributeDto createDto)
        {
            // بررسی وجود دسته‌بندی
            var category = await _context.Categories.FindAsync(createDto.CategoryId);
            if (category == null)
                throw new InvalidOperationException("دسته‌بندی یافت نشد.");

            // بررسی تکراری نبودن نام ویژگی در همان دسته
            var exists = await _context.CategoryAttributes
                .AnyAsync(a => a.CategoryId == createDto.CategoryId && a.Name == createDto.Name);
            if (exists)
                throw new InvalidOperationException($"ویژگی '{createDto.Name}' قبلاً در این دسته‌بندی وجود دارد.");

            var attribute = new CategoryAttribute
            {
                CategoryId = createDto.CategoryId,
                Name = createDto.Name,
                Type = createDto.Type,
                IsRequired = createDto.IsRequired,
                IsFilterable = createDto.IsFilterable,
                Options = createDto.Options,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.Now
            };

            await _context.CategoryAttributes.AddAsync(attribute);
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryAttributeDto>(attribute);
        }

        public async Task<CategoryAttributeDto> UpdateAttributeAsync(int id, UpdateCategoryAttributeDto updateDto)
        {
            var attribute = await _context.CategoryAttributes
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attribute == null)
                throw new KeyNotFoundException("ویژگی یافت نشد.");

            if (!string.IsNullOrWhiteSpace(updateDto.Name))
                attribute.Name = updateDto.Name;

            if (updateDto.CategoryId.HasValue)
                attribute.CategoryId = updateDto.CategoryId.Value;

            if (updateDto.Type.HasValue)
                attribute.Type = updateDto.Type.Value;

            if (updateDto.IsRequired.HasValue)
                attribute.IsRequired = updateDto.IsRequired.Value;

            if (updateDto.IsFilterable.HasValue)
                attribute.IsFilterable = updateDto.IsFilterable.Value;

            if (updateDto.Options != null)
                attribute.Options = updateDto.Options;

            attribute.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryAttributeDto>(attribute);
        }

        public async Task<bool> DeleteAttributeAsync(int id)
        {
            var attribute = await _context.CategoryAttributes
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attribute == null)
                return false;

            // بررسی اینکه ویژگی در محصولات استفاده نشده باشد
            var isUsed = await _context.ProductAttributeValues
                .AnyAsync(pav => pav.AttributeId == id);
            if (isUsed)
                throw new InvalidOperationException("این ویژگی در محصولات استفاده شده است و قابل حذف نیست.");

            _context.CategoryAttributes.Remove(attribute);
            await _context.SaveChangesAsync();
            return true;
        }

        // 🔧 متدهای کمکی خصوصی

        private static string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // 1️⃣ تبدیل حروف فارسی به انگلیسی
            var slug = ConvertPersianToEnglish(name);

            // 2️⃣ تبدیل به حروف کوچک
            slug = slug.ToLower();

            // 3️⃣ جایگزینی فاصله با خط تیره
            slug = slug.Replace(" ", "-");

            // 4️⃣ حذف کاراکترهای غیرمجاز (فقط حروف انگلیسی، اعداد و خط تیره)
            slug = Regex.Replace(slug, @"[^a-z0-9-]", "");

            // 5️⃣ حذف خط تیره‌های اضافی (اگر چندین خط تیره پشت سر هم آمد)
            slug = Regex.Replace(slug, @"-+", "-");

            // 6️⃣ حذف خط تیره از ابتدا و انتها
            slug = slug.Trim('-');

            return slug;
        }

        private static string ConvertPersianToEnglish(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // حروف فارسی به انگلیسی
            var persianToEnglishMap = new Dictionary<char, char>
            {
                // حروف الفبا
                {'ا', 'a'}, {'ب', 'b'}, {'پ', 'p'}, {'ت', 't'}, {'ث', 's'},
                {'ج', 'j'}, {'چ', 'c'}, {'ح', 'h'}, {'خ', 'x'}, {'د', 'd'},
                {'ذ', 'z'}, {'ر', 'r'}, {'ز', 'z'}, {'ژ', 'j'}, {'س', 's'},
                {'ش', 's'}, {'ص', 's'}, {'ض', 'z'}, {'ط', 't'}, {'ظ', 'z'},
                {'ع', 'a'}, {'غ', 'g'}, {'ف', 'f'}, {'ق', 'q'}, {'ک', 'k'},
                {'گ', 'g'}, {'ل', 'l'}, {'م', 'm'}, {'ن', 'n'}, {'و', 'v'},
                {'ه', 'h'}, {'ی', 'y'},
                // اعداد فارسی به انگلیسی
                {'۰', '0'}, {'۱', '1'}, {'۲', '2'}, {'۳', '3'}, {'۴', '4'},
                {'۵', '5'}, {'۶', '6'}, {'۷', '7'}, {'۸', '8'}, {'۹', '9'}
            };

            var result = new System.Text.StringBuilder();
            foreach (var ch in input)
            {
                if (persianToEnglishMap.TryGetValue(ch, out var englishChar))
                {
                    result.Append(englishChar);
                }
                else
                {
                    result.Append(ch);
                }
            }

            return result.ToString();
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
