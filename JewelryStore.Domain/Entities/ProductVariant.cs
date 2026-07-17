using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string VariantName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string VariantAttributes { get; set; } = string.Empty; // JSON or comma-separated

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public decimal PriceAdjustment { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;


        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
