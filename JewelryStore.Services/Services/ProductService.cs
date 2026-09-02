using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Admin;
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

            foreach (var dto in productDtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                dto.IsInStock = product.Quantity > 0;
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

        public async Task<int> GetTotalProductsCountAsync(AdminProductFilterDto filter)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.Trim();
                query = query.Where(p => p.Name.Contains(search) || (p.Brand != null && p.Brand.Contains(search)));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive.Value);

            if (filter.IsInStock.HasValue)
                query = filter.IsInStock.Value ? query.Where(p => p.Quantity > 0) : query.Where(p => p.Quantity <= 0);

            return await query.CountAsync();
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
                // 1️⃣ اعتبارسنجی اولیه
                if (string.IsNullOrWhiteSpace(createDto.Name))
                    throw new ArgumentException("نام محصول الزامی است.");

                if (createDto.CategoryId <= 0)
                    throw new ArgumentException("دسته‌بندی محصول الزامی است.");

                if (createDto.BasePrice <= 0)
                    throw new ArgumentException("قیمت محصول باید بیشتر از صفر باشد.");

                // 2️⃣ تولید Slug
                string slug;
                if (!string.IsNullOrWhiteSpace(createDto.Slug))
                {
                    slug = GenerateSlug(createDto.Slug);
                }
                else
                {
                    slug = GenerateSlug(createDto.Name);
                }

                // 3️⃣ بررسی یکتا بودن Slug
                if (await _context.Products.AnyAsync(p => p.Slug == slug))
                {
                    slug = $"{slug}-{Guid.NewGuid().ToString().Substring(0, 6)}";
                }

                // 4️⃣ محاسبه قیمت نهایی
                var finalPrice = CalculateFinalPrice(createDto.BasePrice, createDto.DiscountPercentage);

                // 5️⃣ ایجاد محصول
                var product = new Product
                {
                    Name = createDto.Name,
                    Slug = slug,
                    CategoryId = createDto.CategoryId,
                    Brand = createDto.Brand,
                    Description = createDto.Description ?? string.Empty,
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

                // ==================== افزودن تصاویر ====================
                if (createDto.ImageFiles != null && createDto.ImageFiles.Any())
                {
                    var productImages = new List<ProductImage>();
                    foreach (var imageFile in createDto.ImageFiles)
                    {
                        if (imageFile != null && imageFile.Length > 0)
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
                    }

                    if (productImages.Any())
                    {
                        await _context.ProductImages.AddRangeAsync(productImages);
                        await _context.SaveChangesAsync();
                    }
                }

                // ==================== افزودن تگ‌ها ====================
                if (createDto.Tags != null && createDto.Tags.Any())
                {
                    await AddTagsToProduct(product.Id, createDto.Tags);
                    await _context.SaveChangesAsync(); // ✅ ذخیره تگ‌ها و ProductTag
                }

                // ==================== افزودن تنوع‌ها (Variants) ====================
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
                    await _context.SaveChangesAsync();
                }

                // ==================== افزودن ویژگی‌های پویا (Attributes) ====================
                if (createDto.Attributes != null && createDto.Attributes.Any())
                {
                    var attributeValues = createDto.Attributes
                        .Where(a => a.Key > 0 && !string.IsNullOrWhiteSpace(a.Value))
                        .Select(a => new ProductAttributeValue
                        {
                            ProductId = product.Id,
                            AttributeId = a.Key,
                            Value = a.Value,
                            CreatedAt = DateTime.Now
                        });

                    if (attributeValues.Any())
                    {
                        await _context.ProductAttributeValues.AddRangeAsync(attributeValues);
                        await _context.SaveChangesAsync();
                    }
                }

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
                // 1️⃣ به‌روزرسانی فیلدهای اصلی
                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                {
                    product.Name = updateDto.Name;
                    if (!string.IsNullOrWhiteSpace(updateDto.Slug))
                    {
                        var newSlug = GenerateSlug(updateDto.Slug);
                        if (await _context.Products.AnyAsync(p => p.Slug == newSlug && p.Id != id))
                            throw new InvalidOperationException($"Slug '{newSlug}' قبلاً استفاده شده است.");
                        product.Slug = newSlug;
                    }
                    else
                    {
                        product.Slug = GenerateSlug(updateDto.Name);
                    }
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

                // 2️⃣ به‌روزرسانی تگ‌ها
                if (updateDto.Tags != null)
                {
                    // حذف تگ‌های قبلی
                    _context.ProductTags.RemoveRange(product.ProductTags);

                    // افزودن تگ‌های جدید
                    var validTags = updateDto.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
                    if (validTags.Any())
                    {
                        await AddTagsToProduct(product.Id, validTags);
                    }

                    await _context.SaveChangesAsync();
                }

                // 3️⃣ به‌روزرسانی تنوع‌ها (Variants)
                if (updateDto.Variants != null)
                {
                    await UpdateVariants(product.Id, updateDto.Variants);
                }

                // 4️⃣ به‌روزرسانی ویژگی‌های پویا (Attributes)
                if (updateDto.Attributes != null)
                {
                    // حذف ویژگی‌های قبلی
                    _context.ProductAttributeValues.RemoveRange(product.AttributeValues);

                    // افزودن ویژگی‌های جدید
                    var attributeValues = updateDto.Attributes
                        .Where(a => a.Key > 0 && !string.IsNullOrWhiteSpace(a.Value))
                        .Select(a => new ProductAttributeValue
                        {
                            ProductId = product.Id,
                            AttributeId = a.Key,
                            Value = a.Value,
                            CreatedAt = DateTime.Now
                        });

                    if (attributeValues.Any())
                    {
                        await _context.ProductAttributeValues.AddRangeAsync(attributeValues);
                    }
                }

                // 5️⃣ به‌روزرسانی تصاویر (اگر تصاویر جدید ارسال شده باشد)
                if (updateDto.ImageFiles != null && updateDto.ImageFiles.Any())
                {
                    // حذف تصاویر قدیمی
                    var oldImages = product.Images.ToList();
                    foreach (var oldImage in oldImages)
                    {
                        DeleteFile(oldImage.ImageUrl);
                        _context.ProductImages.Remove(oldImage);
                    }

                    // افزودن تصاویر جدید
                    var productImages = new List<ProductImage>();
                    foreach (var imageFile in updateDto.ImageFiles)
                    {
                        if (imageFile != null && imageFile.Length > 0)
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
                    }

                    if (productImages.Any())
                    {
                        await _context.ProductImages.AddRangeAsync(productImages);
                    }
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

            if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.StartsWith("/"))
            {
                imageUrl = "/" + imageUrl;
            }

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
            var validTags = tagNames.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
            if (!validTags.Any()) return;

            // 1️⃣ دریافت تگ‌های موجود
            var existingTags = await _context.Tags
                .Where(t => validTags.Contains(t.Name))
                .ToListAsync();

            // 2️⃣ پیدا کردن تگ‌های جدید
            var newTagNames = validTags.Except(existingTags.Select(t => t.Name)).ToList();

            // 3️⃣ ایجاد تگ‌های جدید
            var newTags = newTagNames.Select(name => new Tag
            {
                Name = name,
                Slug = GenerateSlug(name),
                CreatedAt = DateTime.Now
            }).ToList();

            if (newTags.Any())
            {
                await _context.Tags.AddRangeAsync(newTags);
                await _context.SaveChangesAsync(); // ✅ ذخیره تگ‌های جدید برای دریافت Id
            }

            // 4️⃣ ترکیب تگ‌های موجود و جدید
            var allTags = existingTags.Concat(newTags).ToList();

            // 5️⃣ ایجاد ProductTag برای هر تگ
            var productTags = allTags.Select(tag => new ProductTag
            {
                ProductId = productId,
                TagId = tag.Id
            }).ToList();

            if (productTags.Any())
            {
                await _context.ProductTags.AddRangeAsync(productTags);
                // ❌ اینجا SaveChanges صدا نزنید، چون در متد اصلی صدا زده می‌شود
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

            var images = product.Images.OrderBy(i => i.DisplayOrder).ToList();

            // ✅ اصلاح: اطمینان از وجود اسلش در تمام تصاویر
            dto.ImageUrls = images.Select(i =>
            {
                var url = i.ImageUrl;
                if (!string.IsNullOrEmpty(url) && !url.StartsWith("/"))
                    url = "/" + url;
                return url;
            }).ToList();

            var mainImage = images.FirstOrDefault(i => i.IsMain) ?? images.FirstOrDefault();

            if (mainImage != null)
            {
                var url = mainImage.ImageUrl;
                if (!string.IsNullOrEmpty(url) && !url.StartsWith("/"))
                    url = "/" + url;
                dto.MainImageUrl = url;
            }
            else
            {
                dto.MainImageUrl = "/images/no-image.png";
            }

            dto.Variants = _mapper.Map<List<ProductVariantDto>>(product.Variants.Where(v => v.IsActive));
            dto.Tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList();
            dto.Attributes = product.AttributeValues
                .ToDictionary(av => av.Attribute.Name, av => av.Value);
            dto.IsInStock = product.Quantity > 0;
            dto.ViewCount = product.ViewCount;

            return dto;
        }
        private void SetMainImages(List<Product> products, IEnumerable<ProductListDto> dtos)
        {
            foreach (var dto in dtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                var mainImage = product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsMain)
                                ?? product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault();

                if (mainImage != null)
                {
                    var url = mainImage.ImageUrl;
                    if (!string.IsNullOrEmpty(url) && !url.StartsWith("/"))
                        url = "/" + url;
                    dto.MainImageUrl = url;
                }
                else
                {
                    dto.MainImageUrl = "/images/no-image.png";
                }
            }
        }
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
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");

            // 5️⃣ حذف خط تیره‌های اضافی
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

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
                relativePath = "/" + relativePath;
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
