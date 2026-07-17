using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public int? UserId { get; set; } // null = admin

        [Required]
        public int OrderId { get; set; }

        public NotificationType Type { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(11)]
        public string PhoneNumber { get; set; } = string.Empty;

        public bool IsSent { get; set; } = false;

        public DateTime? SentAt { get; set; }

        [MaxLength(500)]
        public string? Error { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; } = null!;
    }
}
