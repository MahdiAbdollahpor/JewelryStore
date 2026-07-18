using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Admin
{
    public class ShippingSettingsDto
    {
        public decimal ShippingCost { get; set; }
        public decimal? FreeShippingThreshold { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateShippingSettingsDto
    {
        public decimal? ShippingCost { get; set; }
        public decimal? FreeShippingThreshold { get; set; }
        public bool? IsActive { get; set; }
    }
}
