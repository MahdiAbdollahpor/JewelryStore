using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.User;

namespace JewelryStore.Services.Interfaces
{
    public interface IUserService
    {
        // عملیات عمومی
        Task<UserProfileDto> RegisterAsync(RegisterDto registerDto);
        Task<LoginResultDto> LoginAsync(LoginDto loginDto);
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto updateDto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> VerifyPhoneAsync(int userId, string verificationCode);

        // عملیات ادمین
        Task<IEnumerable<UserListDto>> GetAllUsersAsync(UserFilterDto filter);
        Task<bool> ChangeUserRoleAsync(int userId, UserRole newRole);
        Task<bool> ToggleUserStatusAsync(int userId);
    }
}
