using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? VariantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty; // Snapshot

        [MaxLength(500)]
        public string? ProductImage { get; set; } // Snapshot

        [MaxLength(100)]
        public string? VariantName { get; set; } // Snapshot

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; } // Snapshot

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal FinalUnitPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey(nameof(VariantId))]
        public virtual ProductVariant? Variant { get; set; }
    }
}
