using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Product;
using Microsoft.AspNetCore.Http;

namespace JewelryStore.Services.Interfaces
{
    public interface IProductService
    {
        // عملیات عمومی
        Task<(IEnumerable<ProductListDto> Products, int TotalCount)> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> GetProductBySlugAsync(string slug);
        Task<IEnumerable<ProductListDto>> GetFeaturedProductsAsync(int count);
        Task<IEnumerable<ProductListDto>> GetNewProductsAsync(int count);
        Task<IEnumerable<ProductListDto>> GetRelatedProductsAsync(int productId, int count);
        Task<IEnumerable<ProductListDto>> SearchProductsAsync(string searchTerm);

        // عملیات ادمین
        Task<ProductDto> CreateProductAsync(CreateProductDto createDto);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateDto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> ToggleProductStatusAsync(int id);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<ProductImage> AddProductImageAsync(int productId, string imageFile, bool isMain = false);
        Task<bool> RemoveProductImageAsync(int imageId);
        Task<bool> ReorderImagesAsync(int productId, List<int> imageIdsInOrder);
    }
}
