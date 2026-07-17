using JewelryStore.Services.DTOs.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface INotificationService
    {  
        /// ارسال پیامک پرداخت موفق به کاربر
        Task<bool> SendPaymentSuccessNotificationAsync(int orderId);

        /// ارسال پیامک ارسال سفارش به کاربر (همراه با کد رهگیری)
        Task<bool> SendOrderShippedNotificationAsync(int orderId, string trackingCode);

        /// ارسال پیامک تحویل سفارش به کاربر
        Task<bool> SendOrderDeliveredNotificationAsync(int orderId);

        /// ارسال پیامک پرداخت جدید به ادمین
        Task<bool> SendAdminPaymentNotificationAsync(int orderId);

        /// دریافت تاریخچه اعلان‌ها (برای ادمین)
        Task<IEnumerable<NotificationHistoryDto>> GetNotificationHistoryAsync(int? orderId = null, bool? isSent = null, int page = 1, int pageSize = 20);
    }
}
