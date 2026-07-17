using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Sms
{
    public class SmsResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TransactionId { get; set; }
        public string? Message { get; set; }
        public int RetStatus { get; set; } // کد وضعیت از سرویس
        public string? ResponseValue { get; set; } // مقدار پاسخ از سرویس
        public string? To { get; set; } // شماره دریافت‌کننده
        public string? Text { get; set; } // متن ارسال‌شده
        public DateTime SentAt { get; set; } // زمان ارسال
        public int AttemptCount { get; set; } // تعداد تلاش‌ها
    }
}
