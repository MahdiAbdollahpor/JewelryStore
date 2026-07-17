using JewelryStore.Services.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JewelryStore.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int? userId, string? sessionId);
        Task<CartItemDto> AddToCartAsync(int? userId, string? sessionId, AddToCartDto addDto);
        Task<CartItemDto> UpdateCartItemAsync(int userId, UpdateCartItemDto updateDto);
        Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
        Task<bool> ClearCartAsync(int userId);
        Task<int> GetCartItemsCountAsync(int? userId, string? sessionId);
        Task<decimal> GetCartTotalAsync(int? userId, string? sessionId);
        Task<bool> MergeCartAsync(int userId, string sessionId);
        Task<bool> ValidateCartAsync(int userId);
    }
}
