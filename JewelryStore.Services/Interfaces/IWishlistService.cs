using JewelryStore.Services.DTOs.Wishlist;

namespace JewelryStore.Services.Interfaces
{
    public interface IWishlistService
    {
        /// <summary>
        /// دریافت لیست علاقه‌مندی‌های یک کاربر
        /// </summary>
        Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(int userId);

        /// <summary>
        /// افزودن محصول به علاقه‌مندی‌ها
        /// </summary>
        Task<bool> AddToWishlistAsync(int userId, int productId);

        /// <summary>
        /// حذف محصول از علاقه‌مندی‌ها
        /// </summary>
        Task<bool> RemoveFromWishlistAsync(int userId, int productId);

        /// <summary>
        /// بررسی وجود محصول در علاقه‌مندی‌های کاربر
        /// </summary>
        Task<bool> IsInWishlistAsync(int userId, int productId);

        /// <summary>
        /// تعداد علاقه‌مندی‌های کاربر
        /// </summary>
        Task<int> GetWishlistCountAsync(int userId);

        /// <summary>
        /// خالی کردن لیست علاقه‌مندی‌های کاربر
        /// </summary>
        Task<bool> ClearWishlistAsync(int userId);

     
    }
}
