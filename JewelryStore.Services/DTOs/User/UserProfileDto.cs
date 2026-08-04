using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.User
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string Role { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsActive { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
