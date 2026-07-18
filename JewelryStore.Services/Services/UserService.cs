using AutoMapper;
using JewelryStore.Data.Context;
using JewelryStore.Domain.Entities;
using JewelryStore.Domain.Enums;
using JewelryStore.Services.DTOs.User;
using JewelryStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace JewelryStore.Services.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ISmsSender _smsSender;
        private readonly IFileStorageService _fileStorageService;

        public UserService(ApplicationDbContext context, IMapper mapper, ISmsSender smsSender, IFileStorageService fileStorageService)
        {
            _context = context;
            _mapper = mapper;
            _smsSender = smsSender;
            _fileStorageService = fileStorageService;
        }

        public async Task<RegisterResultDto> RegisterAsync(RegisterDto registerDto)
        {
            // بررسی یکتا بودن شماره
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == registerDto.PhoneNumber);

            if (existingUser != null)
            {
                // اگر کاربر قبلاً ثبت‌نام کرده ولی تایید نکرده، کد جدید بفرست
                if (!existingUser.IsPhoneVerified)
                {
                    existingUser.VerificationCode = GenerateVerificationCode();
                    existingUser.VerificationCodeExpiry = DateTime.Now.AddMinutes(5);
                    await _context.SaveChangesAsync();

                    // ارسال پیامک
                    SendVerificationSms(existingUser.PhoneNumber, existingUser.VerificationCode, existingUser.FullName);

                    return new RegisterResultDto
                    {
                        IsSuccess = true,
                        Message = "کد تایید مجدداً به شماره شما ارسال شد.",
                        RequiresVerification = true,
                        PhoneNumber = registerDto.PhoneNumber
                    };
                }

                return new RegisterResultDto
                {
                    IsSuccess = false,
                    Message = "شماره موبایل قبلاً ثبت شده است.",
                    RequiresVerification = false
                };
            }

            // ایجاد کاربر جدید
            var user = new User
            {
                Username = registerDto.Username,
                PhoneNumber = registerDto.PhoneNumber,
                PasswordHash = HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                Role = UserRole.User,
                IsPhoneVerified = false,
                IsActive = true,
                VerificationCode = GenerateVerificationCode(),
                VerificationCodeExpiry = DateTime.Now.AddMinutes(5),
                CreatedAt = DateTime.Now
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // ارسال کد تایید
            SendVerificationSms(user.PhoneNumber, user.VerificationCode, user.FullName);

            return new RegisterResultDto
            {
                IsSuccess = true,
                Message = "کد تایید به شماره موبایل شما ارسال شد.",
                RequiresVerification = true,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<VerifyResultDto> VerifyPhoneAsync(VerifyPhoneDto verifyDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == verifyDto.PhoneNumber);

            if (user == null)
            {
                return new VerifyResultDto
                {
                    IsSuccess = false,
                    Message = "کاربری با این شماره یافت نشد."
                };
            }

            // بررسی انقضای کد
            if (user.VerificationCodeExpiry < DateTime.Now)
            {
                return new VerifyResultDto
                {
                    IsSuccess = false,
                    Message = "کد تایید منقضی شده است. لطفاً کد جدید درخواست کنید.",
                    CodeExpired = true
                };
            }

            // بررسی صحت کد
            if (user.VerificationCode != verifyDto.Code)
            {
                return new VerifyResultDto
                {
                    IsSuccess = false,
                    Message = "کد تایید اشتباه است."
                };
            }

            // تایید نهایی
            user.IsPhoneVerified = true;
            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new VerifyResultDto
            {
                IsSuccess = true,
                Message = "شماره موبایل با موفقیت تایید شد.",
                User = _mapper.Map<UserProfileDto>(user)
            };
        }

        public async Task<bool> ResendVerificationCodeAsync(string phoneNumber)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user == null || user.IsPhoneVerified)
                return false;

            user.VerificationCode = GenerateVerificationCode();
            user.VerificationCodeExpiry = DateTime.Now.AddMinutes(5);
            await _context.SaveChangesAsync();

            SendVerificationSms(user.PhoneNumber, user.VerificationCode, user.FullName);
            return true;
        }

        public async Task<LoginResultDto> LoginWithCodeAsync(string phoneNumber, string code)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user == null)
                return new LoginResultDto { IsSuccess = false, Message = "کاربری با این شماره یافت نشد." };

            if (!user.IsPhoneVerified)
                return new LoginResultDto { IsSuccess = false, Message = "شماره موبایل تایید نشده است." };

            if (user.VerificationCode != code || user.VerificationCodeExpiry < DateTime.Now)
                return new LoginResultDto { IsSuccess = false, Message = "کد اشتباه یا منقضی شده است." };

            // پاک کردن کد بعد از ورود
            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // TODO: تولید توکن JWT (در آینده)
            return new LoginResultDto
            {
                IsSuccess = true,
                Message = "ورود موفقیت‌آمیز بود.",
                User = _mapper.Map<UserProfileDto>(user)
            };
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

        public async Task<bool> UpdateUserAvatarAsync(int userId, IFormFile avatarFile)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            // حذف آواتار قدیمی (اگر وجود داشته باشد)
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                await _fileStorageService.DeleteFileAsync(user.Avatar);
            }

            // آپلود آواتار جدید
            var avatarPath = await _fileStorageService.UploadFileAsync(
                avatarFile,
                "users/avatars",
                $"user-{userId}-{DateTime.Now:yyyyMMdd}"
            );

            user.Avatar = _fileStorageService.GetFileUrl(avatarPath);
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

        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(10000, 99999).ToString();
        }

        private void SendVerificationSms(string phoneNumber, string code, string? fullName)
        {
            // ارسال کد تایید با الگوی ثبت‌نام (type: 1)
            _smsSender.SendSms(
                type: 1,
                phoneNumber: phoneNumber,
                parameters: new[] { fullName ?? "کاربر", code }
            );
        }

    }
}
