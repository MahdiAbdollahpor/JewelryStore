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
    public class OrderStatusHistory : BaseEntity
    {
        [Required]
        public int OrderId { get; set; }

        public OrderStatus Status { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public int? ChangedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey(nameof(ChangedByUserId))]
        public virtual User? ChangedByUser { get; set; }
    }
}
