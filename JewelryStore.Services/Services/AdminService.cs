using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.Admin;
using JewelryStore.Services.DTOs.Order;
using JewelryStore.Services.DTOs.Product;
using JewelryStore.Services.DTOs.Report;
using JewelryStore.Services.DTOs.User;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IDiscountService _discountService;
        private readonly IReportService _reportService;
        private readonly IFileStorageService _fileStorageService;

        public AdminService(
            ApplicationDbContext context,
            IMapper mapper,
            IUserService userService,
            IProductService productService,
            IOrderService orderService,
            IDiscountService discountService,
            IReportService reportService,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _productService = productService;
            _orderService = orderService;
            _discountService = discountService;
            _reportService = reportService;
            _fileStorageService = fileStorageService;
        }

        // ==================== 📊 داشبورد ====================
        public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync()
        {
            return await _reportService.GetDashboardStatisticsAsync();
        }

        // ==================== 👤 مدیریت کاربران ====================
        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync(UserFilterDto filter)
        {
            return await _userService.GetAllUsersAsync(filter);
        }

        public async Task<UserProfileDto> GetUserByIdAsync(int userId)
        {
            return await _userService.GetProfileAsync(userId);
        }

        public async Task<bool> ChangeUserRoleAsync(int userId, string newRole)
        {
            if (!Enum.TryParse<UserRole>(newRole, true, out var role))
                return false;

            return await _userService.ChangeUserRoleAsync(userId, role);
        }

        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            return await _userService.ToggleUserStatusAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // فقط غیرفعال کردن کاربر (حذف نرم)
            user.IsActive = false;
            user.UpdatedAt = DateTime.Now;

            // اگر فیلد IsDeleted را اضافه نکرده‌اید، این خط را حذف کنید
            // user.IsDeleted = true;

            await _context.SaveChangesAsync();
            return true;
        }

        // ==================== 📦 مدیریت محصولات ====================
        public async Task<IEnumerable<ProductListDto>> GetAllProductsAsync(AdminProductFilterDto filter)
        {
            var productFilter = new ProductFilterDto
            {
                SearchTerm = filter.SearchTerm,
                CategoryId = filter.CategoryId,
                OnlyInStock = filter.IsInStock,
                OnlyFeatured = filter.IsFeatured,
                OnlyNew = filter.IsNew,
                Page = filter.Page,
                PageSize = filter.PageSize,
                SortBy = filter.SortBy
            };

            var (products, _) = await _productService.GetProductsAsync(productFilter);
            return products;
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            return await _productService.GetProductByIdAsync(productId);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
        {
            return await _productService.CreateProductAsync(createDto);
        }

        public async Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto updateDto)
        {
            return await _productService.UpdateProductAsync(productId, updateDto);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            return await _productService.DeleteProductAsync(productId);
        }

        public async Task<bool> ToggleProductStatusAsync(int productId)
        {
            return await _productService.ToggleProductStatusAsync(productId);
        }

        public async Task<bool> UpdateProductStockAsync(int productId, int quantity)
        {
            return await _productService.UpdateStockAsync(productId, quantity);
        }

        // ==================== 🛒 مدیریت سفارشات ====================
        public async Task<IEnumerable<OrderListDto>> GetAllOrdersAsync(AdminOrderFilterDto filter)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .AsQueryable();

            // اعمال فیلترها
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.Trim();
                query = query.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    o.User.PhoneNumber.Contains(search) ||
                    (o.User.FullName != null && o.User.FullName.Contains(search)));
            }

            if (filter.OrderStatus.HasValue)
                query = query.Where(o => o.OrderStatus == filter.OrderStatus.Value);

            if (filter.PaymentStatus.HasValue)
                query = query.Where(o => o.PaymentStatus == filter.PaymentStatus.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

            // مرتب‌سازی
            query = filter.SortDescending
                ? query.OrderByDescending(o => o.CreatedAt)
                : query.OrderBy(o => o.CreatedAt);

            // صفحه‌بندی
            var skip = (filter.Page - 1) * filter.PageSize;
            var orders = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            return orders.Select(o => new OrderListDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.User.FullName ?? o.User.Username,
                CustomerPhone = o.User.PhoneNumber,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count
            });
        }

        public async Task<OrderDetailDto> GetOrderByIdAsync(int orderId)
        {
            return await _orderService.GetOrderByIdAsync(orderId);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, string? note = null)
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                return false;

            return await _orderService.UpdateOrderStatusAsync(orderId, orderStatus, note);
        }

        public async Task<bool> AddTrackingCodeAsync(int orderId, string trackingCode)
        {
            return await _orderService.AddTrackingCodeAsync(orderId, trackingCode);
        }

        public async Task<bool> CancelOrderAsync(int orderId, string reason)
        {
            // بررسی وجود سفارش
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            // فقط سفارشاتی که پرداخت نشده یا در وضعیت Pending هستند قابل لغو هستند
            if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Paid)
                return false;

            order.OrderStatus = OrderStatus.Cancelled;
            order.AdminNote = reason;
            order.UpdatedAt = DateTime.Now;

            // ثبت تاریخچه
            var history = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Cancelled,
                Note = $"لغو سفارش به دلیل: {reason}",
                CreatedAt = DateTime.Now
            };
            await _context.OrderStatusHistories.AddAsync(history);

            await _context.SaveChangesAsync();
            return true;
        }

        // ==================== 🎁 مدیریت تخفیف‌ها ====================
        public async Task<IEnumerable<DiscountListDto>> GetAllDiscountsAsync(AdminDiscountFilterDto filter)
        {
            var query = _context.DiscountCodes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.Trim();
                query = query.Where(d =>
                    d.Code.Contains(search) ||
                    d.Title.Contains(search));
            }

            if (filter.IsActive.HasValue)
                query = query.Where(d => d.IsActive == filter.IsActive.Value);

            query = filter.SortDescending
                ? query.OrderByDescending(d => d.CreatedAt)
                : query.OrderBy(d => d.CreatedAt);

            var discounts = await query.ToListAsync();
            return _mapper.Map<IEnumerable<DiscountListDto>>(discounts);
        }

        public async Task<DiscountDto> CreateDiscountAsync(CreateDiscountDto createDto)
        {
            // بررسی یکتا بودن کد
            if (await _context.DiscountCodes.AnyAsync(d => d.Code == createDto.Code))
                throw new InvalidOperationException("کد تخفیف قبلاً وجود دارد.");

            var discount = new DiscountCode
            {
                Code = createDto.Code,
                Title = createDto.Title,
                DiscountType = createDto.DiscountType,
                DiscountValue = createDto.DiscountValue,
                MaxDiscountAmount = createDto.MaxDiscountAmount,
                MinOrderAmount = createDto.MinOrderAmount,
                UsageLimit = createDto.UsageLimit,
                UsagePerUser = createDto.UsagePerUser,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            // تبدیل لیست‌ها به JSON
            if (createDto.ApplicableProducts != null && createDto.ApplicableProducts.Any())
                discount.ApplicableProducts = System.Text.Json.JsonSerializer.Serialize(createDto.ApplicableProducts);

            if (createDto.ApplicableCategories != null && createDto.ApplicableCategories.Any())
                discount.ApplicableCategories = System.Text.Json.JsonSerializer.Serialize(createDto.ApplicableCategories);

            if (createDto.ExcludedProducts != null && createDto.ExcludedProducts.Any())
                discount.ExcludedProducts = System.Text.Json.JsonSerializer.Serialize(createDto.ExcludedProducts);

            await _context.DiscountCodes.AddAsync(discount);
            await _context.SaveChangesAsync();

            return _mapper.Map<DiscountDto>(discount);
        }

        public async Task<DiscountDto> UpdateDiscountAsync(int discountId, UpdateDiscountDto updateDto)
        {
            var discount = await _context.DiscountCodes.FindAsync(discountId);
            if (discount == null)
                throw new KeyNotFoundException("کد تخفیف یافت نشد.");

            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                discount.Title = updateDto.Title;

            if (updateDto.DiscountValue.HasValue)
                discount.DiscountValue = updateDto.DiscountValue.Value;

            if (updateDto.MaxDiscountAmount.HasValue)
                discount.MaxDiscountAmount = updateDto.MaxDiscountAmount.Value;

            if (updateDto.MinOrderAmount.HasValue)
                discount.MinOrderAmount = updateDto.MinOrderAmount.Value;

            if (updateDto.UsageLimit.HasValue)
                discount.UsageLimit = updateDto.UsageLimit.Value;

            if (updateDto.UsagePerUser.HasValue)
                discount.UsagePerUser = updateDto.UsagePerUser.Value;

            if (updateDto.StartDate.HasValue)
                discount.StartDate = updateDto.StartDate.Value;

            if (updateDto.EndDate.HasValue)
                discount.EndDate = updateDto.EndDate.Value;

            discount.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return _mapper.Map<DiscountDto>(discount);
        }

        public async Task<bool> ToggleDiscountStatusAsync(int discountId)
        {
            var discount = await _context.DiscountCodes.FindAsync(discountId);
            if (discount == null)
                return false;

            discount.IsActive = !discount.IsActive;
            discount.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return discount.IsActive;
        }

        public async Task<bool> DeleteDiscountAsync(int discountId)
        {
            var discount = await _context.DiscountCodes.FindAsync(discountId);
            if (discount == null)
                return false;

            _context.DiscountCodes.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        // ==================== ⚙️ تنظیمات سایت ====================
        public async Task<ShippingSettingsDto> GetShippingSettingsAsync()
        {
            var settings = await _context.ShippingSettings
                .FirstOrDefaultAsync(s => s.IsActive);

            if (settings == null)
                return new ShippingSettingsDto
                {
                    ShippingCost = 50000,
                    FreeShippingThreshold = 1000000,
                    IsActive = true
                };

            return new ShippingSettingsDto
            {
                ShippingCost = settings.ShippingCost,
                FreeShippingThreshold = settings.FreeShippingThreshold,
                IsActive = settings.IsActive,
                UpdatedAt = settings.UpdatedAt
            };
        }

        public async Task<bool> UpdateShippingSettingsAsync(UpdateShippingSettingsDto updateDto)
        {
            var settings = await _context.ShippingSettings
                .FirstOrDefaultAsync(s => s.IsActive);

            if (settings == null)
            {
                settings = new ShippingSetting
                {
                    ShippingCost = updateDto.ShippingCost ?? 50000,
                    FreeShippingThreshold = updateDto.FreeShippingThreshold,
                    IsActive = updateDto.IsActive ?? true,
                    CreatedAt = DateTime.Now
                };
                await _context.ShippingSettings.AddAsync(settings);
            }
            else
            {
                if (updateDto.ShippingCost.HasValue)
                    settings.ShippingCost = updateDto.ShippingCost.Value;

                if (updateDto.FreeShippingThreshold.HasValue)
                    settings.FreeShippingThreshold = updateDto.FreeShippingThreshold.Value;

                if (updateDto.IsActive.HasValue)
                    settings.IsActive = updateDto.IsActive.Value;

                settings.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaxSettingsDto> GetTaxSettingsAsync()
        {
            var settings = await _context.TaxSettings
                .FirstOrDefaultAsync(t => t.IsActive);

            if (settings == null)
                return new TaxSettingsDto
                {
                    TaxPercentage = 9,
                    IsActive = true
                };

            return new TaxSettingsDto
            {
                TaxPercentage = settings.TaxPercentage,
                IsActive = settings.IsActive,
                UpdatedAt = settings.UpdatedAt
            };
        }

        public async Task<bool> UpdateTaxSettingsAsync(UpdateTaxSettingsDto updateDto)
        {
            var settings = await _context.TaxSettings
                .FirstOrDefaultAsync(t => t.IsActive);

            if (settings == null)
            {
                settings = new TaxSetting
                {
                    TaxPercentage = updateDto.TaxPercentage ?? 9,
                    IsActive = updateDto.IsActive ?? true,
                    CreatedAt = DateTime.Now
                };
                await _context.TaxSettings.AddAsync(settings);
            }
            else
            {
                if (updateDto.TaxPercentage.HasValue)
                    settings.TaxPercentage = updateDto.TaxPercentage.Value;

                if (updateDto.IsActive.HasValue)
                    settings.IsActive = updateDto.IsActive.Value;

                settings.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }




        public async Task<string> UploadSiteLogoAsync(IFormFile logoFile)
        {
            var logoPath = await _fileStorageService.UploadFileAsync(
                logoFile,
                "site",
                $"logo-{DateTime.Now:yyyyMMdd}"
            );

            return _fileStorageService.GetFileUrl(logoPath);
        }

        /// <summary>
        /// حذف لوگوی سایت
        /// </summary>
        public async Task<bool> DeleteSiteLogoAsync(string logoPath)
        {
            return await _fileStorageService.DeleteFileAsync(logoPath);
        }

        /// <summary>
        /// آپلود تصویر بنر
        /// </summary>
        public async Task<string> UploadBannerImageAsync(IFormFile bannerFile, string bannerName)
        {
            var bannerPath = await _fileStorageService.UploadFileAsync(
                bannerFile,
                "banners",
                $"{bannerName}-{DateTime.Now:yyyyMMdd}"
            );

            return _fileStorageService.GetFileUrl(bannerPath);
        }
    }
}
