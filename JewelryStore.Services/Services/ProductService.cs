using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Product;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JewelryStore.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public ProductService(ApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService)
        {
            _context = context;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        // 1️⃣ دریافت محصولات با فیلتر
        public async Task<(IEnumerable<ProductListDto> Products, int TotalCount)> GetProductsAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            // اعمال فیلترها
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.Trim();
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Brand != null && p.Brand.Contains(search)) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            if (filter.CategoryId.HasValue)
            {
                // شامل دسته اصلی و زیردسته‌های آن
                var categoryIds = await GetCategoryAndSubCategoryIds(filter.CategoryId.Value);
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.FinalPrice >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.FinalPrice <= filter.MaxPrice.Value);

            if (filter.Purities != null && filter.Purities.Any())
                query = query.Where(p => filter.Purities.Contains(p.Purity));

            if (filter.StoneTypes != null && filter.StoneTypes.Any())
                query = query.Where(p => p.StoneType.HasValue && filter.StoneTypes.Contains(p.StoneType.Value));

            if (filter.MinWeight.HasValue)
                query = query.Where(p => p.Weight >= filter.MinWeight.Value);

            if (filter.MaxWeight.HasValue)
                query = query.Where(p => p.Weight <= filter.MaxWeight.Value);

            if (filter.OnlyInStock == true)
                query = query.Where(p => p.Quantity > 0);

            if (filter.OnlyDiscounted == true)
                query = query.Where(p => p.DiscountPercentage > 0);

            if (filter.OnlyFeatured == true)
                query = query.Where(p => p.IsFeatured);

            if (filter.OnlyNew == true)
                query = query.Where(p => p.IsNew);

            // فقط محصولات فعال
            query = query.Where(p => p.IsActive);

            // محاسبه تعداد کل قبل از صفحه‌بندی
            var totalCount = await query.CountAsync();

            // مرتب‌سازی
            query = filter.SortBy?.ToLower() switch
            {
                "priceLowToHigh" => query.OrderBy(p => p.FinalPrice),
                "priceHighToLow" => query.OrderByDescending(p => p.FinalPrice),
                "popularity" => query.OrderByDescending(p => p.ViewCount),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                _ => query.OrderByDescending(p => p.CreatedAt) // Newest
            };

            // صفحه‌بندی
            var skip = (filter.Page - 1) * filter.PageSize;
            var products = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            var productDtos = _mapper.Map<IEnumerable<ProductListDto>>(products);

            // تنظیم تصویر اصلی
            foreach (var dto in productDtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                var mainImage = product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.FirstOrDefault();
                if (mainImage != null)
                    dto.MainImageUrl = mainImage.ImageUrl;
            }

            return (productDtos, totalCount);
        }

        // 2️⃣ دریافت محصول با شناسه
        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.Attribute)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            return MapToProductDto(product);
        }

        // 3️⃣ دریافت محصول با Slug
        public async Task<ProductDto> GetProductBySlugAsync(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.Attribute)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            // افزایش تعداد بازدید
            product.ViewCount++;
            await _context.SaveChangesAsync();

            return MapToProductDto(product);
        }

        // 4️⃣ دریافت محصولات ویژه
        public async Task<IEnumerable<ProductListDto>> GetFeaturedProductsAsync(int count)
        {
            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.IsActive && p.IsFeatured && p.Quantity > 0)
                .OrderBy(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            var productDtos = _mapper.Map<IEnumerable<ProductListDto>>(products);
            SetMainImages(products, productDtos);
            return productDtos;
        }

        // 5️⃣ دریافت محصولات جدید
        public async Task<IEnumerable<ProductListDto>> GetNewProductsAsync(int count)
        {
            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.IsActive && p.IsNew && p.Quantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            var productDtos = _mapper.Map<IEnumerable<ProductListDto>>(products);
            SetMainImages(products, productDtos);
            return productDtos;
        }

        // 6️⃣ دریافت محصولات مرتبط
        public async Task<IEnumerable<ProductListDto>> GetRelatedProductsAsync(int productId, int count)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return Enumerable.Empty<ProductListDto>();

            var relatedProducts = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.IsActive && p.Id != productId && p.CategoryId == product.CategoryId && p.Quantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            var productDtos = _mapper.Map<IEnumerable<ProductListDto>>(relatedProducts);
            SetMainImages(relatedProducts, productDtos);
            return productDtos;
        }

        // 7️⃣ جستجوی محصولات
        public async Task<IEnumerable<ProductListDto>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<ProductListDto>();

            var search = searchTerm.Trim();
            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.IsActive &&
                    (p.Name.Contains(search) ||
                     (p.Brand != null && p.Brand.Contains(search))))
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();

            var productDtos = _mapper.Map<IEnumerable<ProductListDto>>(products);
            SetMainImages(products, productDtos);
            return productDtos;
        }

        // 8️⃣ ایجاد محصول جدید (ادمین)
        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto, List<IFormFile>? imageFiles = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // تولید Slug
                var slug = GenerateSlug(createDto.Name);
                if (await _context.Products.AnyAsync(p => p.Slug == slug))
                    throw new InvalidOperationException($"Slug '{slug}' قبلاً استفاده شده است.");

                // محاسبه قیمت نهایی
                var finalPrice = CalculateFinalPrice(createDto.BasePrice, createDto.DiscountPercentage);

                var product = new Product
                {
                    Name = createDto.Name,
                    Slug = slug,
                    CategoryId = createDto.CategoryId,
                    Brand = createDto.Brand,
                    Description = createDto.Description,
                    ShortDescription = createDto.ShortDescription,
                    BasePrice = createDto.BasePrice,
                    DiscountPercentage = createDto.DiscountPercentage,
                    FinalPrice = finalPrice,
                    Weight = createDto.Weight,
                    Purity = createDto.Purity,
                    GoldPriceReference = createDto.GoldPriceReference,
                    CraftsmanshipFee = createDto.CraftsmanshipFee,
                    StoneType = createDto.StoneType,
                    StoneWeight = createDto.StoneWeight,
                    StoneQuality = createDto.StoneQuality,
                    Quantity = createDto.Quantity,
                    MinOrderQuantity = createDto.MinOrderQuantity,
                    MaxOrderQuantity = createDto.MaxOrderQuantity,
                    IsActive = true,
                    IsFeatured = createDto.IsFeatured,
                    IsNew = createDto.IsNew,
                    CreatedAt = DateTime.Now,
                    PublishedAt = DateTime.Now
                };

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                // ✅ آپلود تصاویر
                if (imageFiles != null && imageFiles.Any())
                {
                    for (int i = 0; i < imageFiles.Count; i++)
                    {
                        var imagePath = await _fileStorageService.UploadFileAsync(
                            imageFiles[i],
                            $"products/{product.Id}",
                            $"{Guid.NewGuid():N}"
                        );

                        var imageUrl = _fileStorageService.GetFileUrl(imagePath);

                        var image = new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = imageUrl,
                            IsMain = i == 0, // اولین تصویر به عنوان اصلی
                            DisplayOrder = i,
                            CreatedAt = DateTime.Now
                        };
                        await _context.ProductImages.AddAsync(image);
                    }
                }

                // افزودن تگ‌ها
                if (createDto.Tags != null && createDto.Tags.Any())
                {
                    await AddTagsToProduct(product.Id, createDto.Tags);
                }

                // افزودن تنوع‌ها
                if (createDto.Variants != null && createDto.Variants.Any())
                {
                    var variants = createDto.Variants.Select(v => new ProductVariant
                    {
                        ProductId = product.Id,
                        VariantName = v.VariantName,
                        VariantAttributes = v.VariantAttributes,
                        Quantity = v.Quantity,
                        PriceAdjustment = v.PriceAdjustment,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                    await _context.ProductVariants.AddRangeAsync(variants);
                }

                // افزودن ویژگی‌های پویا
                if (createDto.Attributes != null && createDto.Attributes.Any())
                {
                    var attributeValues = createDto.Attributes.Select(a => new ProductAttributeValue
                    {
                        ProductId = product.Id,
                        AttributeId = a.Key,
                        Value = a.Value,
                        CreatedAt = DateTime.Now
                    });
                    await _context.ProductAttributeValues.AddRangeAsync(attributeValues);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // دریافت محصول کامل برای بازگشت
                return await GetProductByIdAsync(product.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 9️⃣ ویرایش محصول (ادمین)
        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateDto)
        {
            var product = await _context.Products
                .Include(p => p.ProductTags)
                .Include(p => p.Variants)
                .Include(p => p.AttributeValues)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // به‌روزرسانی فیلدها
                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                {
                    product.Name = updateDto.Name;
                    product.Slug = GenerateSlug(updateDto.Name);
                }

                if (updateDto.CategoryId.HasValue)
                    product.CategoryId = updateDto.CategoryId.Value;

                if (!string.IsNullOrWhiteSpace(updateDto.Brand))
                    product.Brand = updateDto.Brand;

                if (!string.IsNullOrWhiteSpace(updateDto.Description))
                    product.Description = updateDto.Description;

                if (!string.IsNullOrWhiteSpace(updateDto.ShortDescription))
                    product.ShortDescription = updateDto.ShortDescription;

                if (updateDto.BasePrice.HasValue)
                {
                    product.BasePrice = updateDto.BasePrice.Value;
                    product.FinalPrice = CalculateFinalPrice(updateDto.BasePrice.Value, product.DiscountPercentage);
                }

                if (updateDto.DiscountPercentage.HasValue)
                {
                    product.DiscountPercentage = updateDto.DiscountPercentage.Value;
                    product.FinalPrice = CalculateFinalPrice(product.BasePrice, updateDto.DiscountPercentage.Value);
                }

                if (updateDto.Weight.HasValue)
                    product.Weight = updateDto.Weight.Value;

                if (updateDto.Purity.HasValue)
                    product.Purity = updateDto.Purity.Value;

                if (updateDto.CraftsmanshipFee.HasValue)
                    product.CraftsmanshipFee = updateDto.CraftsmanshipFee.Value;

                if (updateDto.StoneType.HasValue)
                    product.StoneType = updateDto.StoneType.Value;

                if (updateDto.StoneWeight.HasValue)
                    product.StoneWeight = updateDto.StoneWeight.Value;

                if (updateDto.StoneQuality.HasValue)
                    product.StoneQuality = updateDto.StoneQuality.Value;

                if (updateDto.Quantity.HasValue)
                    product.Quantity = updateDto.Quantity.Value;

                if (updateDto.MinOrderQuantity.HasValue)
                    product.MinOrderQuantity = updateDto.MinOrderQuantity.Value;

                if (updateDto.MaxOrderQuantity.HasValue)
                    product.MaxOrderQuantity = updateDto.MaxOrderQuantity.Value;

                if (updateDto.IsActive.HasValue)
                    product.IsActive = updateDto.IsActive.Value;

                if (updateDto.IsFeatured.HasValue)
                    product.IsFeatured = updateDto.IsFeatured.Value;

                if (updateDto.IsNew.HasValue)
                    product.IsNew = updateDto.IsNew.Value;

                product.UpdatedAt = DateTime.Now;

                // به‌روزرسانی تگ‌ها
                if (updateDto.Tags != null)
                {
                    // حذف تگ‌های قبلی
                    _context.ProductTags.RemoveRange(product.ProductTags);
                    // افزودن تگ‌های جدید
                    await AddTagsToProduct(product.Id, updateDto.Tags);
                }

                // به‌روزرسانی تنوع‌ها
                if (updateDto.Variants != null)
                {
                    await UpdateVariants(product.Id, updateDto.Variants);
                }

                // به‌روزرسانی ویژگی‌های پویا
                if (updateDto.Attributes != null)
                {
                    // حذف ویژگی‌های قبلی
                    _context.ProductAttributeValues.RemoveRange(product.AttributeValues);
                    // افزودن ویژگی‌های جدید
                    var attributeValues = updateDto.Attributes.Select(a => new ProductAttributeValue
                    {
                        ProductId = product.Id,
                        AttributeId = a.Key,
                        Value = a.Value,
                        CreatedAt = DateTime.Now
                    });
                    await _context.ProductAttributeValues.AddRangeAsync(attributeValues);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetProductByIdAsync(product.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 🔟 حذف محصول (ادمین - حذف نرم)
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            // بررسی وجود سفارشات
            var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
            if (hasOrders)
                throw new InvalidOperationException("این محصول در سفارشات استفاده شده است و قابل حذف نیست.");

            product.IsActive = false;
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        // 1️⃣1️⃣ فعال/غیرفعال کردن محصول
        public async Task<bool> ToggleProductStatusAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            product.IsActive = !product.IsActive;
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return product.IsActive;
        }

        // 1️⃣2️⃣ به‌روزرسانی موجودی
        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            product.Quantity = quantity;
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        // 1️⃣3️⃣ افزودن تصویر به محصول
        public async Task<ProductImage> AddProductImageAsync(int productId, IFormFile imageFile, bool isMain = false)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            // آپلود فایل
            var imagePath = await _fileStorageService.UploadFileAsync(
                imageFile,
                $"products/{productId}",
                $"{Guid.NewGuid():N}"
            );

            var imageUrl = _fileStorageService.GetFileUrl(imagePath);

            // ایجاد رکورد در دیتابیس
            var image = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                IsMain = isMain,
                DisplayOrder = 0,
                CreatedAt = DateTime.Now
            };

            // اگر تصویر اصلی است، سایر تصاویر را غیراصلی کن
            if (isMain)
            {
                var existingMain = await _context.ProductImages
                    .FirstOrDefaultAsync(i => i.ProductId == productId && i.IsMain);
                if (existingMain != null)
                    existingMain.IsMain = false;
            }

            await _context.ProductImages.AddAsync(image);
            await _context.SaveChangesAsync();

            return image;
        }

        public async Task<bool> RemoveProductImageAsync(int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null)
                return false;

            // حذف فایل از سرور
            await _fileStorageService.DeleteFileAsync(image.ImageUrl);

            // حذف رکورد از دیتابیس
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            // اگر تصویر اصلی حذف شد، اولین تصویر موجود را به عنوان اصلی انتخاب کن
            if (image.IsMain)
            {
                var nextImage = await _context.ProductImages
                    .Where(i => i.ProductId == image.ProductId)
                    .OrderBy(i => i.DisplayOrder)
                    .FirstOrDefaultAsync();

                if (nextImage != null)
                {
                    nextImage.IsMain = true;
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        // 1️⃣5️⃣ تغییر ترتیب تصاویر
        public async Task<bool> ReorderImagesAsync(int productId, List<int> imageIdsInOrder)
        {
            var images = await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .ToListAsync();

            foreach (var image in images)
            {
                var order = imageIdsInOrder.IndexOf(image.Id);
                if (order >= 0)
                    image.DisplayOrder = order;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // 🔧 متدهای کمکی خصوصی

        private async Task<List<int>> GetCategoryAndSubCategoryIds(int categoryId)
        {
            var ids = new List<int> { categoryId };
            var subCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == categoryId && c.IsActive)
                .Select(c => c.Id)
                .ToListAsync();

            ids.AddRange(subCategories);
            return ids;
        }

        private async Task AddTagsToProduct(int productId, List<string> tagNames)
        {
            var existingTags = await _context.Tags
                .Where(t => tagNames.Contains(t.Name))
                .ToListAsync();

            var newTagNames = tagNames.Except(existingTags.Select(t => t.Name)).ToList();

            // ایجاد تگ‌های جدید
            foreach (var name in newTagNames)
            {
                var tag = new Tag
                {
                    Name = name,
                    Slug = GenerateSlug(name),
                    CreatedAt = DateTime.Now
                };
                await _context.Tags.AddAsync(tag);
                existingTags.Add(tag);
            }

            // ایجاد ارتباط محصول-تگ
            foreach (var tag in existingTags)
            {
                var productTag = new ProductTag
                {
                    ProductId = productId,
                    TagId = tag.Id
                };
                await _context.ProductTags.AddAsync(productTag);
            }
        }

        private async Task UpdateVariants(int productId, List<UpdateProductVariantDto> variantDtos)
        {
            var existingVariants = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            var updatedIds = variantDtos.Where(v => v.Id > 0).Select(v => v.Id).ToList();

            // حذف تنوع‌هایی که در درخواست نیستند
            var variantsToRemove = existingVariants.Where(v => !updatedIds.Contains(v.Id)).ToList();
            if (variantsToRemove.Any())
                _context.ProductVariants.RemoveRange(variantsToRemove);

            // به‌روزرسانی یا ایجاد تنوع‌ها
            foreach (var dto in variantDtos)
            {
                if (dto.Id > 0)
                {
                    // ویرایش تنوع موجود
                    var variant = existingVariants.FirstOrDefault(v => v.Id == dto.Id);
                    if (variant != null)
                    {
                        variant.VariantName = dto.VariantName;
                        variant.VariantAttributes = dto.VariantAttributes;
                        variant.Quantity = dto.Quantity;
                        variant.PriceAdjustment = dto.PriceAdjustment;
                        variant.IsActive = dto.IsActive;
                        variant.UpdatedAt = DateTime.Now;
                    }
                }
                else
                {
                    // ایجاد تنوع جدید
                    var variant = new ProductVariant
                    {
                        ProductId = productId,
                        VariantName = dto.VariantName,
                        VariantAttributes = dto.VariantAttributes,
                        Quantity = dto.Quantity,
                        PriceAdjustment = dto.PriceAdjustment,
                        IsActive = dto.IsActive,
                        CreatedAt = DateTime.Now
                    };
                    await _context.ProductVariants.AddAsync(variant);
                }
            }
        }

        private ProductDto MapToProductDto(Product product)
        {
            var dto = _mapper.Map<ProductDto>(product);
            dto.CategoryName = product.Category?.Name;

            // تصاویر
            dto.ImageUrls = product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList();
            var mainImage = product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.FirstOrDefault();
            dto.MainImageUrl = mainImage?.ImageUrl;

            // تنوع‌ها
            dto.Variants = _mapper.Map<List<ProductVariantDto>>(product.Variants.Where(v => v.IsActive));

            // تگ‌ها
            dto.Tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList();

            // ویژگی‌های پویا
            dto.Attributes = product.AttributeValues
                .ToDictionary(av => av.Attribute.Name, av => av.Value);

            return dto;
        }

        private void SetMainImages(List<Product> products, IEnumerable<ProductListDto> dtos)
        {
            foreach (var dto in dtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                var mainImage = product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.FirstOrDefault();
                if (mainImage != null)
                    dto.MainImageUrl = mainImage.ImageUrl;
            }
        }

        private static string GenerateSlug(string name)
        {
            var slug = name.ToLower().Replace(" ", "-");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");
            return slug;
        }

        private static decimal CalculateFinalPrice(decimal basePrice, decimal discountPercentage)
        {
            var discount = basePrice * (discountPercentage / 100);
            return basePrice - discount;
        }
    }
}
