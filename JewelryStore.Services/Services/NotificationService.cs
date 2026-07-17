using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.Notification;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ISmsSender _smsSender;
        private readonly ApplicationDbContext _context;

        public NotificationService(ISmsSender smsSender, ApplicationDbContext context)
        {
            _smsSender = smsSender;
            _context = context;
        }

        // ✅ اصلاح شده با آرایه
        public async Task<bool> SendPaymentSuccessNotificationAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.User == null)
                return false;

            var parameters = new[]
            {
                order.User.FullName ?? order.User.Username,
                order.TotalAmount.ToString("N0"),
                order.OrderNumber
            };

            return await Task.Run(() => _smsSender.SendSms(
                type: 3,
                phoneNumber: order.User.PhoneNumber,
                parameters: parameters
            ));
        }

        public async Task<bool> SendOrderShippedNotificationAsync(int orderId, string trackingCode)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.User == null)
                return false;

            var parameters = new[]
            {
                order.User.FullName ?? order.User.Username,
                trackingCode
            };

            return await Task.Run(() => _smsSender.SendSms(
                type: 4,
                phoneNumber: order.User.PhoneNumber,
                parameters: parameters
            ));
        }

        public async Task<bool> SendOrderDeliveredNotificationAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.User == null)
                return false;

            var parameters = new[]
            {
                order.User.FullName ?? order.User.Username
            };

            return await Task.Run(() => _smsSender.SendSms(
                type: 5,
                phoneNumber: order.User.PhoneNumber,
                parameters: parameters
            ));
        }

        public async Task<bool> SendAdminPaymentNotificationAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == Domain.Enums.UserRole.Admin && u.IsActive);

            if (admin == null)
                return false;

            string message = $"💰 پرداخت جدید! سفارش {order.OrderNumber} به مبلغ {order.TotalAmount:N0} توسط {order.User?.FullName ?? order.User?.Username}";

            // برای ادمین از یک پیام ساده استفاده می‌کنیم (type=0)
            return await Task.Run(() => _smsSender.SendSms(0, admin.PhoneNumber, message));
        }

        // متد تاریخچه (اختیاری)
        public async Task<IEnumerable<NotificationHistoryDto>> GetNotificationHistoryAsync(
            int? orderId = null, bool? isSent = null, int page = 1, int pageSize = 20)
        {
            // پیاده‌سازی این متد اگر نیاز دارید
            return await Task.FromResult(new List<NotificationHistoryDto>());
        }
    }
}
