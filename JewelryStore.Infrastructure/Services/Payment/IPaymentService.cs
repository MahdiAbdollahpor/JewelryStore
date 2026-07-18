using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Infrastructure.Services.Payment
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
    }

    //public class PaymentResult
    //{
    //    public bool IsSuccess { get; set; }
    //    public string Message { get; set; }
    //    public string Authority { get; set; }
    //    public string PaymentUrl { get; set; }
    //    public long? RefId { get; set; }
    //    public string ErrorCode { get; set; }
    //}
}
