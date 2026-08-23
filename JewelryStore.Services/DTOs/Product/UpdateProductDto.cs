using JewelryStore.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Product
{
    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? Weight { get; set; }
        public Purity? Purity { get; set; }
        public decimal? CraftsmanshipFee { get; set; }
        public StoneType? StoneType { get; set; }
        public decimal? StoneWeight { get; set; }
        public StoneQuality? StoneQuality { get; set; }
        public int? Quantity { get; set; }
        public int? MinOrderQuantity { get; set; }
        public int? MaxOrderQuantity { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsNew { get; set; }
        public List<string>? Tags { get; set; } // تگ‌های جدید
        public List<IFormFile>? ImageFiles { get; set; }
        public List<UpdateProductVariantDto>? Variants { get; set; }
        public List<AttributeValueDto>? Attributes { get; set; } // AttributeId -> Value
    }

    public class UpdateProductVariantDto
    {
        public int Id { get; set; } // اگر 0 باشد یعنی جدید
        public string VariantName { get; set; }
        public string VariantAttributes { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAdjustment { get; set; }
        public bool IsActive { get; set; }
    }
}
