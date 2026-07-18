using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Report
{
    public class OrderStatusDistributionDto
    {
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        public int Count { get; set; }
        public int Percentage { get; set; }
    }
}
