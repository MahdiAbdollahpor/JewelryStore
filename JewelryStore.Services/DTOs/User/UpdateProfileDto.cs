using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.User
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
    }
}
