using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.DTOs.Admin
{
    public class AdminCreateUserDto
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [MaxLength(11)]
        [Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد")]
        public string Password { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        public string? Address { get; set; }

        public string Role { get; set; } = "User";
    }
}
