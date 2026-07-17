using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Order
{
    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public string ShippingAddress { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? DiscountCode { get; set; } // کد تخفیف (اختیاری)
        public string? CustomerNote { get; set; }
    }
}
