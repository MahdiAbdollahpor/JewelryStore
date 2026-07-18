using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.User
{
    public class VerifyResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public bool CodeExpired { get; set; }
        public UserProfileDto? User { get; set; }
    }
}
