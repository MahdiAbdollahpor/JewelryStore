using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Notification
{
    public class SendSmsDto
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public int OrderId { get; set; }
        public int? UserId { get; set; } // null = ارسال به ادمین
    }
}
