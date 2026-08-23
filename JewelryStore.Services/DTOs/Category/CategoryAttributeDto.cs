using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Category
{
    public class CategoryAttributeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
        public bool IsFilterable { get; set; }
        public string? Options { get; set; }
    }
}
