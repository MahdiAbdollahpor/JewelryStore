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
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal BasePrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal FinalPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Weight { get; set; } // Weight in grams

        public Purity Purity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal GoldPriceReference { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CraftsmanshipFee { get; set; }

        public StoneType? StoneType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? StoneWeight { get; set; } // Weight in carats

        public StoneQuality? StoneQuality { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public int MinOrderQuantity { get; set; } = 1;

        public int MaxOrderQuantity { get; set; } = 10;

        private bool _isInStock;
        public bool IsInStock
        {
            get => _isInStock;
            private set => _isInStock = value;
        }

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public bool IsNew { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        [Range(0, 5)]
        public decimal AverageRating { get; set; } = 0;

        public int ReviewCount { get; set; } = 0;

        public DateTime? PublishedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();

        public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
