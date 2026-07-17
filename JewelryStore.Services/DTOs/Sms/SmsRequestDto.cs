using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Sms
{
    public class SmsRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string To { get; set; } // برای ارسال به یک نفر
        public string[] ToArray { get; set; } // برای ارسال به چند نفر
        public string From { get; set; }
        public string Text { get; set; }
        public bool IsFlash { get; set; } = false;
    }
}
