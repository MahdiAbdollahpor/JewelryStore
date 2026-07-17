using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Order
{
    public class OrderResultDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentUrl { get; set; } // برای اتصال به درگاه
        public string Message { get; set; }
    }
}
