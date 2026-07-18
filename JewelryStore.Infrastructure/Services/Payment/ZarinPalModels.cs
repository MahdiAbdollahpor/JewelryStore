
using Newtonsoft.Json;

namespace JewelryStore.Infrastructure.Services.Payment
{
    public class ZarinPalRequest
    {
        [JsonProperty("merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("callback_url")]
        public string CallbackUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("metadata")]
        public ZarinPalMetadata Metadata { get; set; }
    }

    public class ZarinPalMetadata
    {
        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    // 2️⃣ پاسخ درخواست پرداخت
    public class ZarinPalRequestResponse
    {
        [JsonProperty("data")]
        public ZarinPalRequestData Data { get; set; }

        [JsonProperty("errors")]
        public object Errors { get; set; }
    }

    public class ZarinPalRequestData
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("authority")]
        public string Authority { get; set; }

        [JsonProperty("fee_type")]
        public string FeeType { get; set; }

        [JsonProperty("fee")]
        public int Fee { get; set; }
    }

    // 3️⃣ درخواست تایید پرداخت
    public class ZarinPalVerifyRequest
    {
        [JsonProperty("merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("authority")]
        public string Authority { get; set; }
    }

    // 4️⃣ پاسخ تایید پرداخت
    public class ZarinPalVerifyResponse
    {
        [JsonProperty("data")]
        public ZarinPalVerifyData Data { get; set; }

        [JsonProperty("errors")]
        public object Errors { get; set; }
    }

    public class ZarinPalVerifyData
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("ref_id")]
        public long RefId { get; set; }

        [JsonProperty("fee_type")]
        public string FeeType { get; set; }

        [JsonProperty("fee")]
        public int Fee { get; set; }
    }

    // 5️⃣ نتیجه نهایی برای استفاده در سرویس
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Authority { get; set; }
        public string PaymentUrl { get; set; }
        public long? RefId { get; set; }
        public string ErrorCode { get; set; }
    }
}

