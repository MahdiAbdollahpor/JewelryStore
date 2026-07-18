using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Wishlist;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JewelryStore.Services.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;

        public WishlistService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// دریافت لیست علاقه‌مندی‌های کاربر
        /// </summary>
        public async Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(int userId)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p.Images)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return wishlist.Select(w => new WishlistItemDto
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product.Name,
                ProductImage = w.Product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl,
                Price = w.Product.BasePrice,
                DiscountPercentage = w.Product.DiscountPercentage > 0 ? w.Product.DiscountPercentage : null,
                FinalPrice = w.Product.FinalPrice,
                Slug = w.Product.Slug,
                IsInStock = w.Product.Quantity > 0,
                AddedAt = w.CreatedAt
            });
        }

        /// <summary>
        /// افزودن محصول به علاقه‌مندی‌ها
        /// </summary>
        public async Task<bool> AddToWishlistAsync(int userId, int productId)
        {
            // بررسی وجود محصول
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

            if (product == null)
                return false;

            // بررسی وجود کاربر
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (user == null)
                return false;

            // بررسی وجود آیتم تکراری
            var existing = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (existing != null)
                return false;

            // افزودن به لیست علاقه‌مندی‌ها
            var wishlistItem = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.Now
            };

            await _context.Wishlists.AddAsync(wishlistItem);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// حذف محصول از علاقه‌مندی‌ها
        /// </summary>
        public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
        {
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem == null)
                return false;

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// بررسی وجود محصول در علاقه‌مندی‌های کاربر
        /// </summary>
        public async Task<bool> IsInWishlistAsync(int userId, int productId)
        {
            return await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        /// <summary>
        /// تعداد علاقه‌مندی‌های کاربر
        /// </summary>
        public async Task<int> GetWishlistCountAsync(int userId)
        {
            return await _context.Wishlists
                .CountAsync(w => w.UserId == userId);
        }

        /// <summary>
        /// خالی کردن لیست علاقه‌مندی‌های کاربر
        /// </summary>
        public async Task<bool> ClearWishlistAsync(int userId)
        {
            var wishlistItems = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (!wishlistItems.Any())
                return false;

            _context.Wishlists.RemoveRange(wishlistItems);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
