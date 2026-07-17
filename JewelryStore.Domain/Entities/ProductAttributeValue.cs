using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class ProductAttributeValue : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int AttributeId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey(nameof(AttributeId))]
        public virtual CategoryAttribute Attribute { get; set; } = null!;
    }
}
