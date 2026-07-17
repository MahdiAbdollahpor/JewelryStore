using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Sms
{
    public class SmsResponseDto
    {
        public string Value { get; set; } // شناسه پیام یا کد خطا
        public int RetStatus { get; set; } // وضعیت: 0 موفقیت
        public string StrRetStatus { get; set; } // توضیح وضعیت
    }
}
