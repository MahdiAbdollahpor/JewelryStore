using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class DiscountCode : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public DiscountType DiscountType { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinOrderAmount { get; set; }

        public int? UsageLimit { get; set; }

        public int? UsagePerUser { get; set; }

        public int UsedCount { get; set; } = 0;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string? ApplicableProducts { get; set; } // JSON array of product IDs

        public string? ApplicableCategories { get; set; } // JSON array of category IDs

        public string? ExcludedProducts { get; set; } // JSON array of product IDs

        // Navigation Properties
        public virtual ICollection<DiscountUsage> Usages { get; set; } = new List<DiscountUsage>();
    }
}
