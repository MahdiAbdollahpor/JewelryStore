using JewelryStore.Services.DTOs.Admin;
using JewelryStore.Services.DTOs.Order;
using JewelryStore.Services.DTOs.Product;
using JewelryStore.Services.DTOs.Report;
using JewelryStore.Services.DTOs.User;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface IAdminService
    {
        // 📊 داشبورد
        Task<DashboardStatisticsDto> GetDashboardStatisticsAsync();

        // 👤 مدیریت کاربران
        Task<IEnumerable<UserListDto>> GetAllUsersAsync(UserFilterDto filter);
        Task<UserProfileDto> GetUserByIdAsync(int userId);
        Task<bool> ChangeUserRoleAsync(int userId, string newRole);
        Task<bool> ToggleUserStatusAsync(int userId);
        Task<bool> DeleteUserAsync(int userId);
        Task<int> GetTotalUsersCountAsync(UserFilterDto filter);
        Task<UserProfileDto> CreateUserByAdminAsync(AdminCreateUserDto createDto);


        // 📦 مدیریت محصولات
        Task<IEnumerable<ProductListDto>> GetAllProductsAsync(AdminProductFilterDto filter);
        Task<ProductDto> GetProductByIdAsync(int productId);
        Task<ProductDto> CreateProductAsync(CreateProductDto createDto);
        Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto updateDto);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> ToggleProductStatusAsync(int productId);
        Task<bool> UpdateProductStockAsync(int productId, int quantity);

        // 🛒 مدیریت سفارشات
        Task<IEnumerable<OrderListDto>> GetAllOrdersAsync(AdminOrderFilterDto filter);
        Task<OrderDetailDto> GetOrderByIdAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status, string? note = null);
        Task<bool> AddTrackingCodeAsync(int orderId, string trackingCode);
        Task<bool> CancelOrderAsync(int orderId, string reason);

        // 🎁 مدیریت تخفیف‌ها
        Task<IEnumerable<DiscountListDto>> GetAllDiscountsAsync(AdminDiscountFilterDto filter);
        Task<DiscountDto> CreateDiscountAsync(CreateDiscountDto createDto);
        Task<DiscountDto> UpdateDiscountAsync(int discountId, UpdateDiscountDto updateDto);
        Task<bool> ToggleDiscountStatusAsync(int discountId);
        Task<bool> DeleteDiscountAsync(int discountId);

        // ⚙️ تنظیمات سایت
        Task<ShippingSettingsDto> GetShippingSettingsAsync();
        Task<bool> UpdateShippingSettingsAsync(UpdateShippingSettingsDto updateDto);
        Task<TaxSettingsDto> GetTaxSettingsAsync();
        Task<bool> UpdateTaxSettingsAsync(UpdateTaxSettingsDto updateDto);


        // 📁 مدیریت فایل‌ها
       
    }
}
