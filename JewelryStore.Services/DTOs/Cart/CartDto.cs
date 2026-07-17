using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Cart
{
    public class CartDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? SessionId { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public int TotalItemsCount => Items.Sum(i => i.Quantity);
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);
        public decimal TotalDiscount => Items.Sum(i => i.DiscountAmount * i.Quantity);
        public decimal TotalPrice => Items.Sum(i => i.FinalUnitPrice * i.Quantity);
        public DateTime? ExpiryDate { get; set; }
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductImage { get; set; }
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalUnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsInStock { get; set; }
        public int MaxOrderQuantity { get; set; }
    }
}
