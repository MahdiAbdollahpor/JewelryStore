using AutoMapper;
using JewelryStore.Domain.Entities;
using JewelryStore.Services.DTOs.Admin;
using JewelryStore.Services.DTOs.Cart;
using JewelryStore.Services.DTOs.Category;
using JewelryStore.Services.DTOs.Order;
using JewelryStore.Services.DTOs.Product;
using JewelryStore.Services.DTOs.User;

namespace JewelryStore.Web
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ==================== User Mappings ====================
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt));

            CreateMap<User, UserListDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.OrderCount, opt => opt.Ignore());

            // ==================== Category Mappings ====================
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentCategoryName,
                    opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null));

            CreateMap<Category, CategoryTreeDto>();

            CreateMap<UpdateCategoryDto, Category>()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ==================== Product Mappings ====================
            CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.MainImageUrl, opt => opt.Ignore())
    .ForMember(dest => dest.ImageUrls, opt => opt.Ignore())
    .ForMember(dest => dest.Variants, opt => opt.Ignore())
    .ForMember(dest => dest.Tags, opt => opt.Ignore())
    .ForMember(dest => dest.Attributes, opt => opt.Ignore())
    .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
    .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.ViewCount));

            CreateMap<Product, ProductListDto>()
                .ForMember(dest => dest.MainImageUrl, opt => opt.Ignore());

            CreateMap<ProductVariant, ProductVariantDto>();

            // ==================== Cart Mappings ====================
            CreateMap<Cart, CartDto>();
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImage, opt => opt.Ignore())
                .ForMember(dest => dest.VariantName, opt => opt.Ignore())
                .ForMember(dest => dest.IsInStock, opt => opt.Ignore())
                .ForMember(dest => dest.MaxOrderQuantity, opt => opt.Ignore());

            // ==================== Order Mappings ====================
            CreateMap<Order, OrderDetailDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.SubTotal))
                .ForMember(dest => dest.DiscountTotal, opt => opt.MapFrom(src => src.DiscountTotal))
                .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => src.ShippingCost))
                .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount))
                .ForMember(dest => dest.DiscountCodeAmount, opt => opt.MapFrom(src => src.DiscountCodeAmount))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate))
                .ForMember(dest => dest.ShippingDate, opt => opt.MapFrom(src => src.ShippingDate))
                .ForMember(dest => dest.DeliveryDate, opt => opt.MapFrom(src => src.DeliveryDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            CreateMap<OrderItem, OrderItemDetailDto>();

            // ==================== Admin Mappings ====================

            CreateMap<CategoryAttribute, CategoryAttributeDto>()
     .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : ""));

            CreateMap<DiscountCode, DiscountListDto>();
            CreateMap<DiscountCode, DiscountDto>();
            CreateMap<CreateDiscountDto, DiscountCode>()
                .ForMember(dest => dest.UsedCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicableProducts, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicableCategories, opt => opt.Ignore())
                .ForMember(dest => dest.ExcludedProducts, opt => opt.Ignore());

            CreateMap<UpdateDiscountDto, DiscountCode>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
