using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Sms
{
    public class SmsCreditResult
    {
        public bool IsSuccess { get; set; }
        public decimal Credit { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
