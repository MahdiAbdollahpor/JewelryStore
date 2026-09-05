using JewelryStore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace JewelryStore.Infrastructure.Services.Payment
{
    public class ZarinPalService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ZarinPalService> _logger;
        private readonly string _merchantId;
        private readonly bool _isSandbox;

        public ZarinPalService(HttpClient httpClient, ILogger<ZarinPalService> logger, string merchantId, bool isSandbox = true)
        {
            _httpClient = httpClient;
            _logger = logger;
            _merchantId = merchantId;
            _isSandbox = isSandbox;

            var baseUrl = _isSandbox
                ? "https://sandbox.zarinpal.com/"
                : "https://api.zarinpal.com/";

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // ==================== درخواست پرداخت ====================
        public async Task<PaymentResult> RequestPaymentAsync(
            int amount,
            string description,
            string callbackUrl,
            string mobile = null,
            string email = null)
        {
            try
            {
                _logger.LogInformation($"درخواست پرداخت به مبلغ {amount} تومان");

                var request = new ZarinPalRequest
                {
                    MerchantId = _merchantId,
                    Amount = amount,
                    CallbackUrl = callbackUrl,
                    Description = description,
                    Metadata = new ZarinPalMetadata
                    {
                        Mobile = mobile,
                        Email = email
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(request, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("pg/v4/payment/request.json", content);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogDebug($"پاسخ زرین‌پال: {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"خطا در ارتباط با زرین‌پال: {response.StatusCode}");
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Message = "خطا در ارتباط با درگاه پرداخت",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }

                var result = JsonConvert.DeserializeObject<ZarinPalRequestResponse>(responseString);

                if (result?.Data == null)
                {
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Message = "پاسخ نامعتبر از درگاه پرداخت",
                        ErrorCode = "INVALID_RESPONSE"
                    };
                }

                if (result.Data.Code != 100)
                {
                    var errorMessage = GetErrorMessage(result.Data.Code, result.Data.Message);
                    _logger.LogWarning($"خطا در درخواست پرداخت: کد {result.Data.Code} - {errorMessage}");
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Message = errorMessage,
                        ErrorCode = result.Data.Code.ToString(),
                        Authority = result.Data.Authority
                    };
                }

                var paymentBaseUrl = _isSandbox
                    ? "https://sandbox.zarinpal.com/pg/StartPay/"
                    : "https://www.zarinpal.com/pg/StartPay/";

                var paymentUrl = $"{paymentBaseUrl}{result.Data.Authority}";

                _logger.LogInformation($"درخواست پرداخت با موفقیت ایجاد شد. Authority: {result.Data.Authority}");

                return new PaymentResult
                {
                    IsSuccess = true,
                    Message = "درخواست پرداخت با موفقیت ایجاد شد",
                    Authority = result.Data.Authority,
                    PaymentUrl = paymentUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در درخواست پرداخت");
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = $"خطا در درخواست پرداخت: {ex.Message}",
                    ErrorCode = "EXCEPTION"
                };
            }
        }

        // ==================== تایید پرداخت ====================
        public async Task<PaymentResult> VerifyPaymentAsync(string authority, int amount)
        {
            try
            {
                _logger.LogInformation($"تایید پرداخت با Authority: {authority}");

                var request = new ZarinPalVerifyRequest
                {
                    MerchantId = _merchantId,
                    Amount = amount,
                    Authority = authority
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("pg/v4/payment/verify.json", content);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogDebug($"پاسخ تایید زرین‌پال: {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"خطا در ارتباط با زرین‌پال برای تایید: {response.StatusCode}");
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Message = "خطا در ارتباط با درگاه پرداخت",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }

                var result = JsonConvert.DeserializeObject<ZarinPalVerifyResponse>(responseString);

                if (result?.Data == null)
                {
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Message = "پاسخ نامعتبر از درگاه پرداخت",
                        Authority = authority,
                        ErrorCode = "INVALID_RESPONSE"
                    };
                }

                if (result.Data.Code == 100 || result.Data.Code == 101)
                {
                    _logger.LogInformation($"پرداخت با موفقیت تایید شد. RefId: {result.Data.RefId}");
                    return new PaymentResult
                    {
                        IsSuccess = true,
                        Message = "پرداخت با موفقیت تایید شد",
                        Authority = authority,
                        RefId = result.Data.RefId,
                        ErrorCode = result.Data.Code.ToString()
                    };
                }

                var errorMessage = GetErrorMessage(result.Data.Code, result.Data.Message);
                _logger.LogWarning($"خطا در تایید پرداخت: کد {result.Data.Code} - {errorMessage}");

                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = errorMessage,
                    Authority = authority,
                    ErrorCode = result.Data.Code.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تایید پرداخت");
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = $"خطا در تایید پرداخت: {ex.Message}",
                    Authority = authority,
                    ErrorCode = "EXCEPTION"
                };
            }
        }

        // ==================== برگشت وجه (Refund) ====================
        public async Task<PaymentResult> RefundPaymentAsync(string authority, int amount)
        {
            try
            {
                _logger.LogInformation($"درخواست برگشت وجه (Reverse) برای Authority: {authority} به مبلغ {amount}");

                // ✅ اگر در حالت Sandbox هستیم، برگشت وجه را شبیه‌سازی کنید
                if (_isSandbox)
                {
                    _logger.LogWarning("حالت Sandbox: برگشت وجه به صورت شبیه‌سازی انجام می‌شود.");
                    return new PaymentResult
                    {
                        IsSuccess = true,
                        Message = "برگشت وجه با موفقیت انجام شد (Sandbox).",
                        Authority = authority,
                        RefId = DateTime.Now.Ticks
                    };
                }

                // ✅ استفاده از متد reverse.json (نه refund.json)
                var requestData = new
                {
                    merchant_id = _merchantId,
                    authority = authority
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                // ✅ آدرس صحیح: reverse.json
                var response = await _httpClient.PostAsync("pg/v4/payment/reverse.json", content);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"پاسخ برگشت وجه (Reverse): {responseString}");

                // بررسی وضعیت پاسخ
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"خطا در برگشت وجه: {response.StatusCode} - {responseString}");
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Message = $"خطا در برگشت وجه: {response.StatusCode}",
                        Authority = authority,
                        ErrorCode = response.StatusCode.ToString()
                    };
                }

                // Parse پاسخ
                try
                {
                    var result = JsonConvert.DeserializeObject<ZarinPalReverseResponse>(responseString);

                    if (result?.Data != null)
                    {
                        // ✅ کد 100 به معنای موفقیت است (پیام "Reversed")
                        if (result.Data.Code == 100)
                        {
                            _logger.LogInformation($"برگشت وجه با موفقیت انجام شد. Authority: {authority}");
                            return new PaymentResult
                            {
                                IsSuccess = true,
                                Message = "برگشت وجه با موفقیت انجام شد.",
                                Authority = authority,
                                RefId = DateTime.Now.Ticks
                            };
                        }

                        // خطا از سمت زرین‌پال
                        var errorMessage = result.Data.Message ?? GetReverseErrorMessage(result.Data.Code);
                        _logger.LogWarning($"خطا در برگشت وجه: کد {result.Data.Code} - {errorMessage}");

                        return new PaymentResult
                        {
                            IsSuccess = false,
                            Message = $"خطا در برگشت وجه: {errorMessage}",
                            Authority = authority,
                            ErrorCode = result.Data.Code.ToString()
                        };
                    }

                    // اگر خطاهای ارسالی وجود داشته باشد
                    if (result?.Errors != null)
                    {
                        var errorMessage = GetReverseErrorMessageFromErrors(result.Errors);
                        return new PaymentResult
                        {
                            IsSuccess = false,
                            Message = errorMessage,
                            Authority = authority,
                            ErrorCode = "ERRORS"
                        };
                    }

                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Message = "پاسخ نامعتبر از درگاه پرداخت.",
                        Authority = authority,
                        ErrorCode = "INVALID_RESPONSE"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطا در Parse پاسخ برگشت وجه");
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Message = $"خطا در برگشت وجه: {ex.Message}",
                        Authority = authority,
                        ErrorCode = "PARSE_ERROR"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در برگشت وجه");
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = $"خطا در برگشت وجه: {ex.Message}",
                    Authority = authority,
                    ErrorCode = "EXCEPTION"
                };
            }
        }

        /// <summary>
        /// ترجمه کدهای خطای متد Reverse
        /// </summary>
        private string GetReverseErrorMessage(int code)
        {
            return code switch
            {
                -60 => "تراکنش قابل برگشت نیست. (ممکن است بیش از ۳۰ دقیقه از پرداخت گذشته باشد)",
                -61 => "تراکنش قبلاً برگشت خورده است.",
                -62 => "آی‌پی سرور مجاز نیست. لطفاً آی‌پی سرور را در تنظیمات درگاه ثبت کنید.",
                -63 => "اطلاعات ارسال شده ناقص است.",
                _ => $"خطای ناشناخته (کد: {code})"
            };
        }

        private string GetReverseErrorMessageFromErrors(object errors)
        {
            try
            {
                var errorJson = JsonConvert.SerializeObject(errors);
                dynamic errorObj = JsonConvert.DeserializeObject(errorJson);
                return errorObj?.message?.ToString() ?? "خطا در برگشت وجه.";
            }
            catch
            {
                return "خطا در برگشت وجه.";
            }
        }

        // ==================== بررسی وضعیت پرداخت ====================
        public async Task<PaymentStatusResult> GetPaymentStatusAsync(string authority)
        {
            try
            {
                // در زرین‌پال، بررسی وضعیت از طریق API موجود است
                // فعلاً یک پاسخ ساده برمی‌گردانیم
                return new PaymentStatusResult
                {
                    IsSuccess = true,
                    Status = "Paid",
                    Message = "وضعیت پرداخت: پرداخت شده"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وضعیت پرداخت");
                return new PaymentStatusResult
                {
                    IsSuccess = false,
                    Status = "Failed",
                    Message = ex.Message
                };
            }
        }

        // ==================== متدهای کمکی ====================
        private string GetErrorMessage(int code, string defaultMessage = null)
        {
            return code switch
            {
                -1 => "اطلاعات ارسال شده ناقص است",
                -2 => "مرچنت‌آیدی نامعتبر است",
                -3 => "مبلغ پرداخت نامعتبر است",
                -4 => "کالبک‌یوآر‌ال نامعتبر است",
                -5 => "آدرس آی‌پی نامعتبر است",
                -6 => "درخواست پرداخت قبلاً ثبت شده است",
                -7 => "پرداخت قبلاً تایید شده است",
                -9 => "کاربر پرداخت را لغو کرده است",
                -10 => "تراکنش ناموفق بوده است",
                100 => "پرداخت با موفقیت انجام شد",
                101 => "پرداخت قبلاً تایید شده است",
                _ => defaultMessage ?? $"خطای ناشناخته (کد: {code})"
            };
        }
    }
}
