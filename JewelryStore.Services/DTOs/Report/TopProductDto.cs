using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Report
{
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? MainImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
