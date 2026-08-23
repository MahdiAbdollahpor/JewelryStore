using JewelryStore.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Product
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string? Slug { get; set; }
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public string Description { get; set; }
        public string? ShortDescription { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal Weight { get; set; }
        public Purity Purity { get; set; }
        public decimal GoldPriceReference { get; set; }
        public decimal CraftsmanshipFee { get; set; }
        public StoneType? StoneType { get; set; }
        public decimal? StoneWeight { get; set; }
        public StoneQuality? StoneQuality { get; set; }
        public int Quantity { get; set; } = 0;
        public int MinOrderQuantity { get; set; } = 1;
        public int MaxOrderQuantity { get; set; } = 10;
        public bool IsFeatured { get; set; } = false;
        public bool IsNew { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public List<string>? Tags { get; set; }
        public List<CreateProductVariantDto>? Variants { get; set; }
        public List<AttributeValueDto>? Attributes { get; set; } // AttributeId -> Value
        public List<string>? ImageUrls { get; set; } // لیست آدرس تصاویر

        public List<IFormFile>? ImageFiles { get; set; }
    }


    public class AttributeValueDto
    {
        public int Key { get; set; }   // AttributeId
        public string Value { get; set; }
    }

    public class CreateProductVariantDto
    {
        public string VariantName { get; set; }
        public string VariantAttributes { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAdjustment { get; set; } = 0;
    }
}
