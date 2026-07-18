using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Admin
{
    public class DiscountListDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public int UsedCount { get; set; }
        public int? UsageLimit { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
