using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Admin
{
    public class DiscountDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int? UsagePerUser { get; set; }
        public int UsedCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<int>? ApplicableProducts { get; set; }
        public List<int>? ApplicableCategories { get; set; }
        public List<int>? ExcludedProducts { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDiscountDto
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int? UsagePerUser { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int>? ApplicableProducts { get; set; }
        public List<int>? ApplicableCategories { get; set; }
        public List<int>? ExcludedProducts { get; set; }
    }

    public class UpdateDiscountDto
    {
        public string? Title { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int? UsagePerUser { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
