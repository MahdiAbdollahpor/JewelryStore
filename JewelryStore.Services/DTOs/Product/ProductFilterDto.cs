using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Product
{
    public class ProductFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<Purity>? Purities { get; set; }
        public List<StoneType>? StoneTypes { get; set; }
        public decimal? MinWeight { get; set; }
        public decimal? MaxWeight { get; set; }
        public bool? OnlyInStock { get; set; }
        public bool? OnlyDiscounted { get; set; }
        public bool? OnlyFeatured { get; set; }
        public bool? OnlyNew { get; set; }
        public string? SortBy { get; set; } // Newest, PriceLowToHigh, PriceHighToLow, Popularity, Rating
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
    }
}
