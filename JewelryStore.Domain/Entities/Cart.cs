using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public int? UserId { get; set; }

        [MaxLength(100)]
        public string? SessionId { get; set; }

        public DateTime? ExpiryDate { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
