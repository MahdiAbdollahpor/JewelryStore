using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Product
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string VariantName { get; set; }
        public string VariantAttributes { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAdjustment { get; set; }
        public bool IsActive { get; set; }
    }
}
