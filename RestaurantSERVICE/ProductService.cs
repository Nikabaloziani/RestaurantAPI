using AutoMapper;
using RestaurantDAL.Repositories;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Product;
using RestaurantSERVICE.DTO.ProductFilterDTO;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductService(
            IProductRepository productRepository,
            IMapper mapper,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        public async Task<List<ProductDTO>> GetAllAsync()
        {
            var products = await _productRepository.GetAllWithCategoryAsync();
            return _mapper.Map<List<ProductDTO>>(products);
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdWithCategoryAsync(id);
            if (product == null) return null;

            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<List<ProductDTO>> GetFilteredAsync(ProductFilterDTO filter)
        {
            var products = await _productRepository.GetFilteredAsync(
                filter.Nuts,
                filter.Vegeterian,
                filter.Spiciness,
                filter.CategoryId
            );
            return _mapper.Map<List<ProductDTO>>(products);
        }

        public async Task<ProductDTO> AddAsync(CreateProductDTO dto)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Specified category does not exist!");
            }

            var productEntity = _mapper.Map<Products>(dto);
            await _productRepository.AddAsync(productEntity);

            return _mapper.Map<ProductDTO>(productEntity);
        }

        public async Task<bool> UpdateAsync(UpdateProductDTO dto)
        {
            var existingProduct = await _productRepository.GetByIdAsync(dto.Id);
            if (existingProduct == null) return false;

            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Specified category does not exist!");
            }

            _mapper.Map(dto, existingProduct);
            await _productRepository.UpdateAsync(existingProduct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null) return false;

            await _productRepository.DeleteAsync(id);
            return true;
        }
    }
}