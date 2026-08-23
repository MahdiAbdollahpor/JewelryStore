using JewelryStore.Services.DTOs.Category;

namespace JewelryStore.Services.Interfaces
{
    public interface ICategoryService
    {
        // عملیات عمومی
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(bool onlyActive = true);
        Task<CategoryDto> GetCategoryByIdAsync(int id);
        Task<CategoryDto> GetCategoryBySlugAsync(string slug);
        Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentId);
        Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync();

        // عملیات ادمین
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto);
        Task<IEnumerable<CategoryAttributeDto>> GetAllAttributesAsync();
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> ToggleCategoryStatusAsync(int id);
    }
}
