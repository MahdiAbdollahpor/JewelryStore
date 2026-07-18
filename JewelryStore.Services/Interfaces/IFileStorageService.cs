using JewelryStore.Services.DTOs.FileStorage;
using Microsoft.AspNetCore.Http;


namespace JewelryStore.Services.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// آپلود یک فایل و ذخیره‌سازی آن در مسیر مشخص
        /// </summary>
        /// <param name="file">فایل دریافتی</param>
        /// <param name="folder">پوشه مقصد (مثلاً products, users)</param>
        /// <param name="fileName">نام فایل (اختیاری - در صورت نبود، خودکار تولید می‌شود)</param>
        /// <returns>مسیر نسبی فایل ذخیره‌شده</returns>
        Task<string> UploadFileAsync(IFormFile file, string folder, string? fileName = null);

        /// <summary>
        /// آپلود یک فایل به صورت بایت آرایه (برای سناریوهای خاص)
        /// </summary>
        Task<string> UploadFileAsync(byte[] fileBytes, string folder, string fileName, string contentType);

        /// <summary>
        /// حذف یک فایل از سیستم
        /// </summary>
        Task<bool> DeleteFileAsync(string filePath);

        /// <summary>
        /// دریافت مسیر کامل یک فایل
        /// </summary>
        string GetFileUrl(string filePath);

        /// <summary>
        /// بررسی وجود فایل
        /// </summary>
        bool FileExists(string filePath);

        /// <summary>
        /// دریافت اطلاعات یک فایل
        /// </summary>
        Task<FileInfoDto> GetFileInfoAsync(string filePath);

        /// <summary>
        /// تغییر نام یک فایل
        /// </summary>
        Task<bool> RenameFileAsync(string oldPath, string newName);
    }
}
