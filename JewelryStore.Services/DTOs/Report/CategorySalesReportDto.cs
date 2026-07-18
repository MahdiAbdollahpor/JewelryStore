using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Report
{
    public class CategorySalesReportDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }
}
