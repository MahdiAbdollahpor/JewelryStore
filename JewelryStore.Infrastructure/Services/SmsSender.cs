using JewelryStore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Infrastructure.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly string _apiKey = "YOUR_API_KEY"; // 👈 کلید API خود را وارد کنید
        private readonly string _fromNumber = "0983000505"; // 👈 شماره اختصاصی خود را وارد کنید
        private readonly ILogger<SmsSender> _logger;

        public SmsSender(ILogger<SmsSender> logger)
        {
            _logger = logger;
        }

        public bool SendSms(int type, string phoneNumber, params string[] parameters)
        {
            try
            {
                string patternCode = GetPatternCode(type);
                string url = BuildUrl(patternCode, phoneNumber, parameters);

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                var response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ پیامک با موفقیت به {phoneNumber} ارسال شد. نوع: {type}");
                    return true;
                }

                _logger.LogWarning($"❌ خطا در ارسال پیامک به {phoneNumber}. کد وضعیت: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ خطا در ارسال پیامک به {phoneNumber}");
                return false;
            }
        }

        private string GetPatternCode(int type)
        {
            return type switch
            {
                1 => "75r5fo7l7u",       // ثبت‌نام: نام + کد تایید
                2 => "cj0jdm47kltfy8f",  // فراموشی رمز: کد تایید
                3 => "YOUR_PAYMENT_PATTERN",  // پرداخت موفق: نام + مبلغ + شماره سفارش (کد الگوی خود را وارد کنید)
                4 => "YOUR_SHIPPING_PATTERN", // ارسال سفارش: نام + کد رهگیری (کد الگوی خود را وارد کنید)
                5 => "YOUR_DELIVERY_PATTERN", // تحویل سفارش: نام (کد الگوی خود را وارد کنید)
                _ => throw new ArgumentException($"نوع پیامک {type} نامعتبر است.")
            };
        }

        private string BuildUrl(string patternCode, string phoneNumber, string[] parameters)
        {
            var baseUrl = $"http://ippanel.com:8080/?apikey={_apiKey}&pid={patternCode}&fnum={_fromNumber}&tnum={phoneNumber}";

            var paramBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < parameters.Length; i++)
            {
                paramBuilder.Append($"&p{i + 1}={Uri.EscapeDataString(parameters[i])}");
            }

            return baseUrl + paramBuilder.ToString();
        }


    }
}
