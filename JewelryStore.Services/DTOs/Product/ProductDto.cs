using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Brand { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal Weight { get; set; }
        public Purity Purity { get; set; }
        public decimal GoldPriceReference { get; set; }
        public decimal CraftsmanshipFee { get; set; }
        public StoneType? StoneType { get; set; }
        public decimal? StoneWeight { get; set; }
        public StoneQuality? StoneQuality { get; set; }
        public int Quantity { get; set; }
        public bool IsInStock { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsNew { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string? MainImageUrl { get; set; } // آدرس تصویر اصلی
        public List<string> ImageUrls { get; set; } = new List<string>(); // همه تصاویر
        public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
        public List<string> Tags { get; set; } = new List<string>();
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>(); // ویژگی‌های پویا
    }
}
