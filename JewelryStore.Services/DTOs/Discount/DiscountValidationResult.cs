using JewelryStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Discount
{
    public class DiscountValidationResult
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public DiscountCode? DiscountCode { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
