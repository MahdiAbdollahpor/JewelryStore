using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.Notification;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ISmsService _smsService; // سرویس پیامک را بعداً اضافه می‌کنیم

        public NotificationService(ApplicationDbContext context, IMapper mapper, ISmsService smsService)
        {
            _context = context;
            _mapper = mapper;
            _smsService = smsService;
        }

        // 1️⃣ ارسال پیامک پرداخت موفق به کاربر
        public async Task<bool> SendPaymentSuccessNotificationAsync(int orderId)
        {
            var order = await GetOrderWithUserAsync(orderId);
            if (order == null)
                return false;

            var user = order.User;
            var message = $"✅ پرداخت سفارش شماره {order.OrderNumber} با مبلغ {order.TotalAmount:N0} تومان با موفقیت انجام شد. " +
                          $"سفارش شما در حال آماده‌سازی است.";

            return await SendAndSaveSmsAsync(new SendSmsDto
            {
                PhoneNumber = user.PhoneNumber,
                Message = message,
                Type = NotificationType.PaymentSuccess,
                OrderId = order.Id,
                UserId = user.Id
            });
        }

        // 2️⃣ ارسال پیامک ارسال سفارش به کاربر (همراه با کد رهگیری)
        public async Task<bool> SendOrderShippedNotificationAsync(int orderId, string trackingCode)
        {
            var order = await GetOrderWithUserAsync(orderId);
            if (order == null)
                return false;

            var user = order.User;
            var message = $"📦 سفارش شماره {order.OrderNumber} ارسال شد. کد رهگیری: {trackingCode}. " +
                          $"لطفاً تا ۲۴ ساعت آینده منتظر دریافت سفارش خود باشید.";

            return await SendAndSaveSmsAsync(new SendSmsDto
            {
                PhoneNumber = user.PhoneNumber,
                Message = message,
                Type = NotificationType.OrderShipped,
                OrderId = order.Id,
                UserId = user.Id
            });
        }

        // 3️⃣ ارسال پیامک تحویل سفارش به کاربر
        public async Task<bool> SendOrderDeliveredNotificationAsync(int orderId)
        {
            var order = await GetOrderWithUserAsync(orderId);
            if (order == null)
                return false;

            var user = order.User;
            var message = $"🎁 سفارش شماره {order.OrderNumber} با موفقیت تحویل داده شد. " +
                          $"از خرید شما متشکریم! منتظر نظرات شما هستیم.";

            return await SendAndSaveSmsAsync(new SendSmsDto
            {
                PhoneNumber = user.PhoneNumber,
                Message = message,
                Type = NotificationType.OrderDelivered,
                OrderId = order.Id,
                UserId = user.Id
            });
        }

        // 4️⃣ ارسال پیامک پرداخت جدید به ادمین
        public async Task<bool> SendAdminPaymentNotificationAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            // پیدا کردن شماره تماس ادمین (فرض می‌کنیم اولین ادمین فعال را بگیریم)
            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == UserRole.Admin && u.IsActive);

            if (admin == null)
                return false;

            var message = $"💰 پرداخت جدید! سفارش شماره {order.OrderNumber} به مبلغ {order.TotalAmount:N0} " +
                          $"توسط کاربر {order.User.FullName ?? order.User.Username} پرداخت شد.";

            return await SendAndSaveSmsAsync(new SendSmsDto
            {
                PhoneNumber = admin.PhoneNumber,
                Message = message,
                Type = NotificationType.AdminPayment,
                OrderId = order.Id,
                UserId = null // null به معنای ارسال به ادمین است
            });
        }

        // 5️⃣ دریافت تاریخچه اعلان‌ها (برای ادمین)
        public async Task<IEnumerable<NotificationHistoryDto>> GetNotificationHistoryAsync(
            int? orderId = null, bool? isSent = null, int page = 1, int pageSize = 20)
        {
            var query = _context.Notifications
                .Include(n => n.User)
                .Include(n => n.Order)
                .AsQueryable();

            if (orderId.HasValue)
                query = query.Where(n => n.OrderId == orderId.Value);

            if (isSent.HasValue)
                query = query.Where(n => n.IsSent == isSent.Value);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<NotificationHistoryDto>();
            foreach (var notif in notifications)
            {
                result.Add(new NotificationHistoryDto
                {
                    Id = notif.Id,
                    UserName = notif.User?.FullName ?? notif.User?.Username ?? "ادمین",
                    UserPhone = notif.User?.PhoneNumber ?? "ادمین",
                    OrderId = notif.OrderId,
                    OrderNumber = notif.Order.OrderNumber,
                    Type = notif.Type,
                    Message = notif.Message,
                    RecipientPhone = notif.PhoneNumber,
                    IsSent = notif.IsSent,
                    CreatedAt = notif.CreatedAt,
                    SentAt = notif.SentAt,
                    Error = notif.Error
                });
            }

            return result;
        }

        // 🔧 متدهای کمکی خصوصی

        private async Task<Order?> GetOrderWithUserAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        private async Task<bool> SendAndSaveSmsAsync(SendSmsDto smsDto)
        {
            var notification = new Notification
            {
                UserId = smsDto.UserId,
                OrderId = smsDto.OrderId,
                Type = smsDto.Type,
                Message = smsDto.Message,
                PhoneNumber = smsDto.PhoneNumber,
                IsSent = false,
                CreatedAt = DateTime.Now
            };

            try
            {
                // ارسال پیامک واقعی (سرویس پیامک را بعداً پیاده‌سازی می‌کنیم)
                var result = await _smsService.SendAsync(smsDto.PhoneNumber, smsDto.Message);

                notification.IsSent = result.IsSuccess;
                notification.SentAt = DateTime.Now;
                notification.Error = result.ErrorMessage;
            }
            catch (Exception ex)
            {
                notification.IsSent = false;
                notification.Error = ex.Message;
            }

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            return notification.IsSent;
        }

       
    }
}
