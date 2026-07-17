using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class TaxSetting : BaseEntity
    {
        [Required]
        [Range(0, 100)]
        public decimal TaxPercentage { get; set; }

        public bool IsActive { get; set; } = true;

        public int? UpdatedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual User? UpdatedByUser { get; set; }
    }
}
