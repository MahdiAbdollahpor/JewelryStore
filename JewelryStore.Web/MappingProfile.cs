using AutoMapper;
using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Cart;
using JewelryStore.Services.DTOs.Category;
using JewelryStore.Services.DTOs.Product;
using JewelryStore.Services.DTOs.User;

namespace JewelryStore.Web
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mappings
            CreateMap<User, UserProfileDto>();
            CreateMap<User, UserListDto>();

            // Category Mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentCategoryName,
                    opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null));

            CreateMap<Category, CategoryTreeDto>();

            // Product Mappings
            CreateMap<Product, ProductDto>();
            CreateMap<Product, ProductListDto>();
            CreateMap<ProductVariant, ProductVariantDto>();

            // Cart Mappings
            CreateMap<Cart, CartDto>();
            CreateMap<CartItem, CartItemDto>();
        }
    }
}
