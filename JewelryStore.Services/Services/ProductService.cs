using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Product;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace JewelryStore.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _environment;

        public ProductService(ApplicationDbContext context, IMapper mapper, IHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }

        // 1️⃣ دریافت محصولات با فیلتر
        public async Task<(IEnumerable<ProductListDto> Products, int TotalCount)> GetProductsAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            // اعمال فیلترها (همون کد قبلی)
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

            query = query.Where(p => p.IsActive);

            var totalCount = await query.CountAsync();

            query = filter.SortBy?.ToLower() switch
            {
                "priceLowToHigh" => query.OrderBy(p => p.FinalPrice),
                "priceHighToLow" => query.OrderByDescending(p => p.FinalPrice),
                "popularity" => query.OrderByDescending(p => p.ViewCount),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var skip = (filter.Page - 1) * filter.PageSize;
            var products = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            var productDtos = _mapper.Map<IEnumerable<ProductListDto>>(products);

            SetMainImages(products, productDtos);

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
        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var slug = GenerateSlug(createDto.Name);
                if (await _context.Products.AnyAsync(p => p.Slug == slug))
                    throw new InvalidOperationException($"Slug '{slug}' قبلاً استفاده شده است.");

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

                // ✅ آپلود و ذخیره تصاویر
                if (createDto.ImageFiles != null && createDto.ImageFiles.Any())
                {
                    var productImages = new List<ProductImage>();
                    foreach (var imageFile in createDto.ImageFiles)
                    {
                        var uploadedResult = UploadFile(imageFile, "Products");
                        if (uploadedResult.Status)
                        {
                            productImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = uploadedResult.FileNameAddress!,
                                IsMain = productImages.Count == 0,
                                DisplayOrder = productImages.Count,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }

                    if (productImages.Any())
                    {
                        await _context.ProductImages.AddRangeAsync(productImages);
                    }
                }

                // ... بقیه کد (تگ‌ها، تنوع‌ها، ویژگی‌ها) ...

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
        // 9️⃣ ویرایش محصول (ادمین)
        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateDto)
        {
            var product = await _context.Products
                .Include(p => p.ProductTags)
                .Include(p => p.Variants)
                .Include(p => p.AttributeValues)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ... به‌روزرسانی فیلدها (همان کد قبلی) ...

                // ✅ به‌روزرسانی تصاویر (اگر تصاویر جدید ارسال شده باشد)
                if (updateDto.ImageFiles != null && updateDto.ImageFiles.Any())
                {
                    await UpdateProductImages(product, updateDto.ImageFiles);
                }

                // ... بقیه کد ...

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
        // 🔟 حذف محصول (ادمین)
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

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

        // 1️⃣3️⃣ افزودن تصویر به محصول (با مسیر مستقیم)
        public async Task<ProductImage> AddProductImageAsync(int productId, string imageUrl, bool isMain = false)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            var image = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                IsMain = isMain,
                DisplayOrder = 0,
                CreatedAt = DateTime.Now
            };

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

        // 1️⃣4️⃣ حذف تصویر از محصول
        public async Task<bool> RemoveProductImageAsync(int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null)
                return false;

            DeleteFile(image.ImageUrl);
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

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

            var variantsToRemove = existingVariants.Where(v => !updatedIds.Contains(v.Id)).ToList();
            if (variantsToRemove.Any())
                _context.ProductVariants.RemoveRange(variantsToRemove);

            foreach (var dto in variantDtos)
            {
                if (dto.Id > 0)
                {
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
            dto.ImageUrls = product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList();
            var mainImage = product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.FirstOrDefault();
            dto.MainImageUrl = mainImage?.ImageUrl;
            dto.Variants = _mapper.Map<List<ProductVariantDto>>(product.Variants.Where(v => v.IsActive));
            dto.Tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList();
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

        /// <summary>
        /// آپلود فایل در پوشه مشخص (با IHostEnvironment)
        /// </summary>
        private UploadResult UploadFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0 || _environment == null)
                return new UploadResult { Status = false };

            try
            {
                // ✅ استفاده از ContentRootPath به جای WebRootPath
                var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                string folderPath = Path.Combine(wwwrootPath, "Images", folder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                string filePath = Path.Combine(folderPath, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                string relativePath = Path.Combine("Images", folder, uniqueFileName).Replace("\\", "/");

                return new UploadResult
                {
                    Status = true,
                    FileNameAddress = relativePath
                };
            }
            catch (Exception)
            {
                return new UploadResult { Status = false };
            }
        }


        /// <summary>
        /// حذف فایل از سرور (با IHostEnvironment)
        /// </summary>
        private void DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || _environment == null)
                return;

            try
            {
                var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                string fullPath = Path.Combine(wwwrootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // اگر فایل وجود نداشت یا خطایی رخ داد، ادامه بده
            }
        }





        /// <summary>
        /// به‌روزرسانی تصاویر محصول (حذف تصاویر قدیمی و اضافه کردن جدید)
        /// </summary>
        private async Task UpdateProductImages(Product product, List<IFormFile> newImages)
        {
            var existingImages = product.Images.ToList();
            foreach (var image in existingImages)
            {
                DeleteFile(image.ImageUrl);
                _context.ProductImages.Remove(image);
            }

            var productImages = new List<ProductImage>();
            foreach (var imageFile in newImages)
            {
                var uploadedResult = UploadFile(imageFile, "Products");
                if (uploadedResult.Status)
                {
                    productImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = uploadedResult.FileNameAddress!,
                        IsMain = productImages.Count == 0,
                        DisplayOrder = productImages.Count,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            if (productImages.Any())
            {
                await _context.ProductImages.AddRangeAsync(productImages);
            }
        }
    }

    public class UploadResult
    {
        public bool Status { get; set; }
        public string? FileNameAddress { get; set; }
    }
}
