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
    public class Order : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? PaymentReference { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal SubTotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountTotal { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ShippingCost { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public int? DiscountCodeId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountCodeAmount { get; set; } = 0;

        [Required]
        [MaxLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(11)]
        public string RecipientPhone { get; set; } = string.Empty;

        public DateTime? ShippingDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [MaxLength(50)]
        public string? TrackingCode { get; set; }

        [MaxLength(500)]
        public string? CustomerNote { get; set; }

        [MaxLength(500)]
        public string? AdminNote { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(DiscountCodeId))]
        public virtual DiscountCode? DiscountCode { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}
