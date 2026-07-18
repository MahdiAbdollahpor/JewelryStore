using JewelryStore.Services.DTOs.FileStorage;
using JewelryStore.Services.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JewelryStore.Services.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _basePath;

        // پسوندهای مجاز برای تصاویر
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private static readonly long MaxFileSize = 5 * 1024 * 1024; // 5 مگابایت

        public LocalFileStorageService(
            IWebHostEnvironment webHostEnvironment,
            ILogger<LocalFileStorageService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _basePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
        }

        /// <summary>
        /// آپلود فایل از IFormFile
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string folder, string? fileName = null)
        {
            // اعتبارسنجی فایل
            ValidateFile(file);

            // تولید مسیر پوشه
            var folderPath = Path.Combine(_basePath, folder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // تولید نام فایل
            var extension = Path.GetExtension(file.FileName);
            var finalFileName = string.IsNullOrEmpty(fileName)
                ? $"{Guid.NewGuid():N}{extension}"
                : $"{fileName}{extension}";

            var fullPath = Path.Combine(folderPath, finalFileName);

            // ذخیره فایل
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation($"فایل {finalFileName} در مسیر {folder} آپلود شد.");

            // بازگشت مسیر نسبی
            return Path.Combine("uploads", folder, finalFileName).Replace("\\", "/");
        }

        /// <summary>
        /// آپلود فایل از بایت آرایه
        /// </summary>
        public async Task<string> UploadFileAsync(
            byte[] fileBytes, string folder, string fileName, string contentType)
        {
            // اعتبارسنجی حجم
            if (fileBytes.Length > MaxFileSize)
                throw new InvalidOperationException($"حجم فایل نباید بیشتر از {MaxFileSize / 1024 / 1024} مگابایت باشد.");

            var folderPath = Path.Combine(_basePath, folder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(fileName);
            var finalFileName = string.IsNullOrEmpty(extension)
                ? $"{fileName}.{GetExtensionFromContentType(contentType)}"
                : fileName;

            var fullPath = Path.Combine(folderPath, finalFileName);

            await File.WriteAllBytesAsync(fullPath, fileBytes);

            _logger.LogInformation($"فایل {finalFileName} در مسیر {folder} آپلود شد.");

            return Path.Combine("uploads", folder, finalFileName).Replace("\\", "/");
        }

        /// <summary>
        /// حذف فایل
        /// </summary>
        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);

            if (!File.Exists(fullPath))
                return false;

            try
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation($"فایل {filePath} حذف شد.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در حذف فایل {filePath}");
                return false;
            }
        }

        /// <summary>
        /// دریافت URL فایل
        /// </summary>
        public string GetFileUrl(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            // اگر مسیر قبلاً کامل است، همان را برگردان
            if (filePath.StartsWith("http") || filePath.StartsWith("/"))
                return filePath;

            return $"/{filePath.Replace("\\", "/")}";
        }

        /// <summary>
        /// بررسی وجود فایل
        /// </summary>
        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// دریافت اطلاعات فایل
        /// </summary>
        public async Task<FileInfoDto> GetFileInfoAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("مسیر فایل نمی‌تواند خالی باشد.");

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"فایل {filePath} یافت نشد.");

            var fileInfo = new FileInfo(fullPath);

            return await Task.FromResult(new FileInfoDto
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                Extension = fileInfo.Extension,
                ContentType = GetContentType(fileInfo.Extension),
                CreatedAt = fileInfo.CreationTime,
                ModifiedAt = fileInfo.LastWriteTime
            });
        }

        /// <summary>
        /// تغییر نام فایل
        /// </summary>
        public async Task<bool> RenameFileAsync(string oldPath, string newName)
        {
            if (string.IsNullOrEmpty(oldPath) || string.IsNullOrEmpty(newName))
                return false;

            var fullOldPath = Path.Combine(_webHostEnvironment.WebRootPath, oldPath);

            if (!File.Exists(fullOldPath))
                return false;

            var directory = Path.GetDirectoryName(fullOldPath);
            var extension = Path.GetExtension(fullOldPath);
            var newFullPath = Path.Combine(directory!, $"{newName}{extension}");

            try
            {
                await Task.Run(() => File.Move(fullOldPath, newFullPath));
                _logger.LogInformation($"فایل از {oldPath} به {newName} تغییر نام یافت.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در تغییر نام فایل {oldPath}");
                return false;
            }
        }

        // ==================== متدهای کمکی ====================

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("فایل نمی‌تواند خالی باشد.");

            // بررسی حجم
            if (file.Length > MaxFileSize)
                throw new InvalidOperationException($"حجم فایل نباید بیشتر از {MaxFileSize / 1024 / 1024} مگابایت باشد.");

            // بررسی پسوند
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedImageExtensions.Contains(extension))
                throw new InvalidOperationException($"پسوند فایل مجاز نیست. پسوندهای مجاز: {string.Join(", ", AllowedImageExtensions)}");

            // بررسی نوع محتوا (اختیاری - امنیت بیشتر)
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
                throw new InvalidOperationException($"نوع فایل {file.ContentType} مجاز نیست.");
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        private static string GetExtensionFromContentType(string contentType)
        {
            return contentType.ToLower() switch
            {
                "image/jpeg" => "jpg",
                "image/png" => "png",
                "image/gif" => "gif",
                "image/webp" => "webp",
                _ => "bin"
            };
        }
    }
}
