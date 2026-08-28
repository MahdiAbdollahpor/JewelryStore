using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Category
{
    public class CreateCategoryAttributeDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public AttributeType Type { get; set; }     
        public bool IsRequired { get; set; } = false; 
        public bool IsFilterable { get; set; } = true;
        public string? Options { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
