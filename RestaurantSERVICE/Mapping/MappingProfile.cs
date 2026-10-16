using AutoMapper;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Basket;
using RestaurantSERVICE.DTO.Category;
using RestaurantSERVICE.DTO.Order;
using RestaurantSERVICE.DTO.Payment;
using RestaurantSERVICE.DTO.Product;
using RestaurantSERVICE.DTO.Table;

namespace RestaurantSERVICE.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Basket Mapping
            CreateMap<CreateBasketDTO, Basket>();
            CreateMap<UpdateBasketDTO, Basket>();
            CreateMap<Basket, BasketDTO>();
            CreateMap<Basket, BasketDTO>()
    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

            //Category Mapping
            CreateMap<Category, CategoryDTO>();
            CreateMap<CreateCategoryDTO, Category>();
            CreateMap<UpdateCategoryDTO, Category>();

            //Product Mapping
            CreateMap<Products, ProductDTO>();
            CreateMap<CreateProductDTO, Products>();
            CreateMap<UpdateProductDTO, Products>();


            //Table Mapping
            CreateMap<CreateTableDTO, Table>();
            CreateMap<Table, TableDTO>();
            CreateMap<UpdateTableStatusDTO, Table>()
                .ForMember(dest => dest.IsOccupied, opt => opt.MapFrom(src => src.Status == TableStatus.Occupied));

            //Order Mapping
            CreateMap<Order, OrderDTO>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

            //Payment Mapping

            CreateMap<Payment, PaymentDTO>()
    .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method.ToString()))
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        }
    }
}