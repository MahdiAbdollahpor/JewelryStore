using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// درخواست پرداخت جدید
        /// </summary>
        Task<PaymentResult> RequestPaymentAsync(
            int amount,
            string description,
            string callbackUrl,
            string mobile = null,
            string email = null);

        /// <summary>
        /// تایید پرداخت
        /// </summary>
        Task<PaymentResult> VerifyPaymentAsync(string authority, int amount);

        /// <summary>
        /// برگشت وجه (Refund) - فقط برای پرداخت‌های آنلاین
        /// </summary>
        Task<PaymentResult> RefundPaymentAsync(string authority, int amount);

        /// <summary>
        /// بررسی وضعیت پرداخت
        /// </summary>
        Task<PaymentStatusResult> GetPaymentStatusAsync(string authority);
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Authority { get; set; }
        public string PaymentUrl { get; set; }
        public long? RefId { get; set; }
        public string ErrorCode { get; set; }
    }

    public class PaymentStatusResult
    {
        public bool IsSuccess { get; set; }
        public string Status { get; set; } // Paid, Pending, Failed, Refunded
        public string Message { get; set; }
        public long? RefId { get; set; }
    }
}

