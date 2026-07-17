using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class ShippingSetting : BaseEntity
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal ShippingCost { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? FreeShippingThreshold { get; set; }

        public bool IsActive { get; set; } = true;

        public int? UpdatedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual User? UpdatedByUser { get; set; }
    }
}
