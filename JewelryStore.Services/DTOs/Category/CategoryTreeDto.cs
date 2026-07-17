using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Category
{
    public class CategoryTreeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public bool IsActive { get; set; }
        public bool ShowInMenu { get; set; }
        public int DisplayOrder { get; set; }
        public List<CategoryTreeDto> SubCategories { get; set; } = new List<CategoryTreeDto>();
    }
}
