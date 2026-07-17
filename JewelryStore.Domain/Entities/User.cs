using JewelryStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Domain.Entities
{
    public class User : BaseEntity
    {
        [MaxLength(50)]
        [Required]
        
        public string Username { get; set; }

        [MaxLength(11)]
        [Required]
        public string PhoneNumber { get; set; }

        [MaxLength(500)]
        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

       
        public UserRole Role { get; set; }

        public bool IsPhoneVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public string? Address { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<DiscountUsage> DiscountUsages { get; set; } = new List<DiscountUsage>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    }
}
