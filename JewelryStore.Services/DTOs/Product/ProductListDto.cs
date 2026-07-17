using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Product
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? MainImageUrl { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public bool IsInStock { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsNew { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
