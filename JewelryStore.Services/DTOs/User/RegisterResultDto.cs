using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.User
{
    public class RegisterResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public bool RequiresVerification { get; set; }
        public string? PhoneNumber { get; set; }
        public UserProfileDto? User { get; set; }
    }
}
