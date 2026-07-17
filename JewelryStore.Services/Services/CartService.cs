using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Cart;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CartService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // 1️⃣ دریافت سبد خرید
        public async Task<CartDto> GetCartAsync(int? userId, string? sessionId)
        {
            if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
                return new CartDto();

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Variant)
                .FirstOrDefaultAsync(c =>
                    (userId.HasValue && c.UserId == userId.Value) ||
                    (!string.IsNullOrEmpty(sessionId) && c.SessionId == sessionId));

            if (cart == null)
                return new CartDto();

            var cartDto = _mapper.Map<CartDto>(cart);

            // تکمیل اطلاعات محصولات
            foreach (var itemDto in cartDto.Items)
            {
                var cartItem = cart.Items.First(i => i.Id == itemDto.Id);
                itemDto.ProductName = cartItem.Product?.Name ?? "نامشخص";
                var image = cartItem.Product?.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl;
                itemDto.ProductImage = image;
                itemDto.IsInStock = cartItem.Product?.Quantity > 0;
                itemDto.MaxOrderQuantity = cartItem.Product?.MaxOrderQuantity ?? 10;
                if (cartItem.Variant != null)
                {
                    itemDto.VariantName = cartItem.Variant.VariantName;
                }
            }

            return cartDto;
        }

        // 2️⃣ افزودن به سبد خرید
        public async Task<CartItemDto> AddToCartAsync(int? userId, string? sessionId, AddToCartDto addDto)
        {
            // بررسی موجودی محصول
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == addDto.ProductId && p.IsActive);

            if (product == null)
                throw new KeyNotFoundException("محصول یافت نشد.");

            if (product.Quantity < addDto.Quantity)
                throw new InvalidOperationException($"موجودی محصول کافی نیست. موجودی: {product.Quantity}");

            if (addDto.Quantity > product.MaxOrderQuantity)
                throw new InvalidOperationException($"حداکثر تعداد قابل سفارش: {product.MaxOrderQuantity}");

            // بررسی تنوع
            if (addDto.VariantId.HasValue)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == addDto.VariantId.Value && v.IsActive);

                if (variant == null)
                    throw new KeyNotFoundException("تنوع محصول یافت نشد.");

                if (variant.Quantity < addDto.Quantity)
                    throw new InvalidOperationException($"موجودی تنوع کافی نیست. موجودی: {variant.Quantity}");
            }

            // دریافت یا ایجاد سبد خرید
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            // بررسی وجود آیتم مشابه
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cart.Id &&
                    ci.ProductId == addDto.ProductId &&
                    ci.VariantId == addDto.VariantId);

            if (existingItem != null)
            {
                // افزایش تعداد
                var newQuantity = existingItem.Quantity + addDto.Quantity;
                if (newQuantity > product.MaxOrderQuantity)
                    throw new InvalidOperationException($"تعداد کل از حد مجاز بیشتر است. حداکثر: {product.MaxOrderQuantity}");

                if (product.Quantity < newQuantity)
                    throw new InvalidOperationException($"موجودی کافی نیست. موجودی: {product.Quantity}");

                existingItem.Quantity = newQuantity;
                existingItem.TotalPrice = existingItem.FinalUnitPrice * existingItem.Quantity;
                existingItem.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return await MapToCartItemDto(existingItem);
            }

            // ایجاد آیتم جدید
            var finalUnitPrice = CalculateFinalPrice(product.BasePrice, product.DiscountPercentage);
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = addDto.ProductId,
                VariantId = addDto.VariantId,
                Quantity = addDto.Quantity,
                UnitPrice = product.BasePrice,
                DiscountAmount = product.BasePrice - finalUnitPrice,
                FinalUnitPrice = finalUnitPrice,
                TotalPrice = finalUnitPrice * addDto.Quantity,
                CreatedAt = DateTime.Now
            };

            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();

            return await MapToCartItemDto(cartItem);
        }

        // 3️⃣ ویرایش تعداد آیتم سبد خرید
        public async Task<CartItemDto> UpdateCartItemAsync(int userId, UpdateCartItemDto updateDto)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == updateDto.CartItemId);

            if (cartItem == null)
                throw new KeyNotFoundException("آیتمی با این شناسه وجود ندارد.");

            // بررسی دسترسی کاربر به این سبد خرید
            if (cartItem.Cart.UserId != userId)
                throw new UnauthorizedAccessException("شما به این آیتم دسترسی ندارید.");

            if (updateDto.Quantity < 1)
                throw new InvalidOperationException("تعداد باید حداقل 1 باشد.");

            // بررسی موجودی
            if (cartItem.Product == null)
                throw new InvalidOperationException("محصول این آیتم وجود ندارد.");

            if (cartItem.Product.Quantity < updateDto.Quantity)
                throw new InvalidOperationException($"موجودی کافی نیست. موجودی: {cartItem.Product.Quantity}");

            if (updateDto.Quantity > cartItem.Product.MaxOrderQuantity)
                throw new InvalidOperationException($"حداکثر تعداد قابل سفارش: {cartItem.Product.MaxOrderQuantity}");

            cartItem.Quantity = updateDto.Quantity;
            cartItem.TotalPrice = cartItem.FinalUnitPrice * updateDto.Quantity;
            cartItem.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return await MapToCartItemDto(cartItem);
        }

        // 4️⃣ حذف از سبد خرید
        public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
                return false;

            // بررسی دسترسی کاربر
            if (cartItem.Cart.UserId != userId)
                throw new UnauthorizedAccessException("شما به این آیتم دسترسی ندارید.");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return true;
        }

        // 5️⃣ خالی کردن سبد خرید
        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return false;

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return true;
        }

        // 6️⃣ تعداد آیتم‌های سبد خرید
        public async Task<int> GetCartItemsCountAsync(int? userId, string? sessionId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    (userId.HasValue && c.UserId == userId.Value) ||
                    (!string.IsNullOrEmpty(sessionId) && c.SessionId == sessionId));

            if (cart == null)
                return 0;

            return cart.Items.Sum(i => i.Quantity);
        }

        // 7️⃣ جمع کل سبد خرید
        public async Task<decimal> GetCartTotalAsync(int? userId, string? sessionId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    (userId.HasValue && c.UserId == userId.Value) ||
                    (!string.IsNullOrEmpty(sessionId) && c.SessionId == sessionId));

            if (cart == null)
                return 0;

            return cart.Items.Sum(i => i.TotalPrice);
        }

        // 8️⃣ ادغام سبد مهمان با کاربر
        public async Task<bool> MergeCartAsync(int userId, string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return false;

            var guestCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (guestCart == null || !guestCart.Items.Any())
                return false;

            // پیدا کردن سبد کاربر
            var userCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                // اگر کاربر سبدی ندارد، سبد مهمان را به او اختصاص بده
                guestCart.UserId = userId;
                guestCart.SessionId = null;
                await _context.SaveChangesAsync();
                return true;
            }

            // انتقال آیتم‌های سبد مهمان به سبد کاربر
            foreach (var guestItem in guestCart.Items.ToList())
            {
                // بررسی وجود آیتم مشابه در سبد کاربر
                var existingItem = userCart.Items.FirstOrDefault(i =>
                    i.ProductId == guestItem.ProductId &&
                    i.VariantId == guestItem.VariantId);

                if (existingItem != null)
                {
                    // ادغام تعداد
                    existingItem.Quantity += guestItem.Quantity;
                    existingItem.TotalPrice = existingItem.FinalUnitPrice * existingItem.Quantity;
                    existingItem.UpdatedAt = DateTime.Now;
                    _context.CartItems.Remove(guestItem);
                }
                else
                {
                    // انتقال آیتم
                    guestItem.CartId = userCart.Id;
                    guestItem.UpdatedAt = DateTime.Now;
                }
            }

            // حذف سبد مهمان خالی
            _context.Carts.Remove(guestCart);
            await _context.SaveChangesAsync();

            return true;
        }

        // 9️⃣ اعتبارسنجی سبد خرید
        public async Task<bool> ValidateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                return true;

            var isValid = true;
            var itemsToRemove = new List<CartItem>();

            foreach (var item in cart.Items)
            {
                // بررسی موجودی محصول
                if (item.Product == null || item.Product.Quantity < item.Quantity)
                {
                    itemsToRemove.Add(item);
                    isValid = false;
                    continue;
                }

                // بررسی تغییر قیمت (اختیاری - می‌توان حذف کرد)
                var finalPrice = CalculateFinalPrice(item.Product.BasePrice, item.Product.DiscountPercentage);
                if (item.FinalUnitPrice != finalPrice)
                {
                    // به‌روزرسانی قیمت
                    item.FinalUnitPrice = finalPrice;
                    item.TotalPrice = finalPrice * item.Quantity;
                    item.UpdatedAt = DateTime.Now;
                }
            }

            // حذف آیتم‌های نامعتبر
            if (itemsToRemove.Any())
            {
                _context.CartItems.RemoveRange(itemsToRemove);
                await _context.SaveChangesAsync();
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            return isValid;
        }

        // 🔧 متدهای کمکی خصوصی

        private async Task<Cart> GetOrCreateCartAsync(int? userId, string? sessionId)
        {
            Cart? cart = null;

            if (userId.HasValue)
            {
                cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId.Value);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId.Value,
                        CreatedAt = DateTime.Now,
                        ExpiryDate = DateTime.Now.AddDays(7)
                    };
                    await _context.Carts.AddAsync(cart);
                    await _context.SaveChangesAsync();
                }
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        SessionId = sessionId,
                        CreatedAt = DateTime.Now,
                        ExpiryDate = DateTime.Now.AddDays(7)
                    };
                    await _context.Carts.AddAsync(cart);
                    await _context.SaveChangesAsync();
                }
            }

            return cart ?? throw new InvalidOperationException("سبد خرید یافت نشد.");
        }

        private async Task<CartItemDto> MapToCartItemDto(CartItem cartItem)
        {
            var dto = _mapper.Map<CartItemDto>(cartItem);

            // تکمیل اطلاعات از Product و Variant
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

            if (product != null)
            {
                dto.ProductName = product.Name;
                var mainImage = product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl;
                dto.ProductImage = mainImage;
                dto.IsInStock = product.Quantity > 0;
                dto.MaxOrderQuantity = product.MaxOrderQuantity;
            }

            if (cartItem.VariantId.HasValue)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == cartItem.VariantId.Value);
                if (variant != null)
                    dto.VariantName = variant.VariantName;
            }

            return dto;
        }

        private static decimal CalculateFinalPrice(decimal basePrice, decimal discountPercentage)
        {
            var discount = basePrice * (discountPercentage / 100);
            return basePrice - discount;
        }
    }
}
