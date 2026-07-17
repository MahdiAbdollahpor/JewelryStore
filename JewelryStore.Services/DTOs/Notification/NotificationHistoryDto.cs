using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Notification
{
    public class NotificationHistoryDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? UserPhone { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public string RecipientPhone { get; set; }
        public bool IsSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public string? Error { get; set; }
    }
}
