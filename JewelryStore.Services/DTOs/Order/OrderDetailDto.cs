using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Order
{
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string? TrackingCode { get; set; }
        public List<OrderItemDetailDto> Items { get; set; }
    }

    public class OrderItemDetailDto
    {
        public string ProductName { get; set; }
        public string? ProductImage { get; set; }
        public string? VariantName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal FinalUnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
