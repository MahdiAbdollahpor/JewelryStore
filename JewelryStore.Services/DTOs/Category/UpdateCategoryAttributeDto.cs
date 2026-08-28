using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Category
{
    public class UpdateCategoryAttributeDto
    {
        public string? Name { get; set; }
        public int? CategoryId { get; set; }         
        public AttributeType? Type { get; set; }     
        public bool? IsRequired { get; set; }         
        public bool? IsFilterable { get; set; }
        public string? Options { get; set; }
    }
}
