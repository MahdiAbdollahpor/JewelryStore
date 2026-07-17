using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class DiscountUsage : BaseEntity
    {
        [Required]
        public int DiscountCodeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int OrderId { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey(nameof(DiscountCodeId))]
        public virtual DiscountCode DiscountCode { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; } = null!;
    }
}
