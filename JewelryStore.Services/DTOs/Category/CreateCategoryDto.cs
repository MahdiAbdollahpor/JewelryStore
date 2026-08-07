using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Category
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string? Slug { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ShowInMenu { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}
