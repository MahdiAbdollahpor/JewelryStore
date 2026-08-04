using JewelryStore.Domain.Entities;
using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface IOrderService
    {
       
        /// ایجاد یک سفارش جدید از سبد خرید کاربر
        Task<OrderResultDto> CreateOrderAsync(CreateOrderDto createDto);

        
        /// دریافت جزئیات یک سفارش با شناسه
        Task<OrderDetailDto> GetOrderByIdAsync(int orderId);


        /// دریافت جزئیات یک سفارش با شماره سفارش
        Task<OrderDetailDto> GetOrderByNumberAsync(string orderNumber);

        /// دریافت لیست سفارش‌های یک کاربر با صفحه‌بندی
        Task<IEnumerable<OrderDetailDto>> GetUserOrdersAsync(int userId, int page = 1, int pageSize = 10);

        /// تغییر وضعیت سفارش (فقط ادمین)
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? note = null);

        /// افزودن کد رهگیری به سفارش (فقط ادمین)
        Task<bool> AddTrackingCodeAsync(int orderId, string trackingCode);
        Task<Order> GetOrderEntityByIdAsync(int orderId);
    }
}
