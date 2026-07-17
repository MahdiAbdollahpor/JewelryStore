using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.User;
using JewelryStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace JewelryStore.Services.Services
{
    public class UserService : IUserService
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserProfileDto> RegisterAsync(RegisterDto registerDto)
        {
            // بررسی یکتا بودن نام کاربری و شماره تماس
            var existingUser = await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.PhoneNumber == registerDto.PhoneNumber);

            if (existingUser)
                throw new InvalidOperationException("نام کاربری یا شماره تماس قبلاً ثبت شده است.");

            var user = new User
            {
                Username = registerDto.Username,
                PhoneNumber = registerDto.PhoneNumber,
                PasswordHash = HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                Role = UserRole.User,
                IsPhoneVerified = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserProfileDto>(user);
        }

        public async Task<LoginResultDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.PhoneNumber);

            if (user == null)
                return new LoginResultDto { IsSuccess = false, Message = "کاربری با این شماره تماس یافت نشد." };

            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                return new LoginResultDto { IsSuccess = false, Message = "رمز عبور اشتباه است." };

            if (!user.IsActive)
                return new LoginResultDto { IsSuccess = false, Message = "حساب کاربری شما غیرفعال است." };

            // به‌روزرسانی تاریخ آخرین ورود
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // TODO: تولید توکن JWT در آینده
            // var token = GenerateJwtToken(user);

            return new LoginResultDto
            {
                IsSuccess = true,
                Message = "ورود موفقیت‌آمیز بود.",
                // Token = token,
                User = _mapper.Map<UserProfileDto>(user)
            };
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            return _mapper.Map<UserProfileDto>(user);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto updateDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            if (!string.IsNullOrWhiteSpace(updateDto.FullName))
                user.FullName = updateDto.FullName;

            if (!string.IsNullOrWhiteSpace(updateDto.Address))
                user.Address = updateDto.Address;

            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return _mapper.Map<UserProfileDto>(user);
        }


        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("رمز عبور فعلی اشتباه است.");

            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> VerifyPhoneAsync(int userId, string verificationCode)
        {
            // در واقعیت باید کد تولید شده را با کد ارسال شده به شماره کاربر بررسی کرد
            // فعلاً یک کد ثابت برای آزمایش در نظر می‌گیریم
            if (verificationCode != "1234")
                return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            user.IsPhoneVerified = true;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync(UserFilterDto filter)
        {
            var query = _context.Users.AsQueryable();

            // اعمال فیلترها
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.Trim();
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.PhoneNumber.Contains(search) ||
                    (u.FullName != null && u.FullName.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Role) && Enum.TryParse<UserRole>(filter.Role, true, out var role))
            {
                query = query.Where(u => u.Role == role);
            }

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            if (filter.IsPhoneVerified.HasValue)
                query = query.Where(u => u.IsPhoneVerified == filter.IsPhoneVerified.Value);

            if (filter.RegisteredFrom.HasValue)
                query = query.Where(u => u.CreatedAt >= filter.RegisteredFrom.Value);

            if (filter.RegisteredTo.HasValue)
                query = query.Where(u => u.CreatedAt <= filter.RegisteredTo.Value);

            // مرتب‌سازی
            query = filter.SortBy?.ToLower() switch
            {
                "username" => filter.SortDescending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
                "phonenumber" => filter.SortDescending ? query.OrderByDescending(u => u.PhoneNumber) : query.OrderBy(u => u.PhoneNumber),
                _ => filter.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };

            // صفحه‌بندی
            var skip = (filter.Page - 1) * filter.PageSize;
            var users = await query.Skip(skip).Take(filter.PageSize).ToListAsync();

            // محاسبه تعداد سفارشات برای هر کاربر
            var userListDtos = new List<UserListDto>();
            foreach (var user in users)
            {
                var orderCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);
                var dto = _mapper.Map<UserListDto>(user);
                dto.OrderCount = orderCount;
                userListDtos.Add(dto);
            }

            return userListDtos;
        }

        public async Task<bool> ChangeUserRoleAsync(int userId, UserRole newRole)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            user.Role = newRole;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return user.IsActive;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }


    }
}
