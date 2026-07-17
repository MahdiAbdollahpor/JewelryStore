using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.User
{
    public class UserFilterDto
    {
        public string? SearchTerm { get; set; } // جستجو در Username, PhoneNumber, FullName
        public string? Role { get; set; } // "User" یا "Admin"
        public bool? IsActive { get; set; }
        public bool? IsPhoneVerified { get; set; }
        public DateTime? RegisteredFrom { get; set; }
        public DateTime? RegisteredTo { get; set; }
        public string? SortBy { get; set; } // "CreatedAt", "Username", "PhoneNumber"
        public bool SortDescending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
