using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Admin
{
    public class TaxSettingsDto
    {
        public decimal TaxPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateTaxSettingsDto
    {
        public decimal? TaxPercentage { get; set; }
        public bool? IsActive { get; set; }
    }
}
