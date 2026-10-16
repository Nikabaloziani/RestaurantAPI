using AutoMapper;
using RestaurantDAL.Repositories;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Order;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            IBasketRepository basketRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        public async Task<OrderDTO> CreateOrderFromBasketAsync(string userId, CreateOrderDTO dto)
        {
            var basketItems = await _basketRepository.GetByUserIdAsync(userId);
            if (!basketItems.Any())
            {
                throw new InvalidOperationException("Basket is empty. Cannot place an order.");
            }

            var order = new Order
            {
                UserId = userId,
                TableId = dto.TableId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = basketItems.Sum(b => b.UnitPrice * b.Quantity),
                OrderItems = basketItems.Select(b => new OrderItem
                {
                    ProductId = b.ProductId,
                    Quantity = b.Quantity,
                    UnitPrice = b.UnitPrice
                }).ToList()
            };

            await _orderRepository.AddAsync(order);

            await _basketRepository.ClearUserBasketAsync(userId);

            var savedOrder = await _orderRepository.GetByIdAsync(order.Id);
            return _mapper.Map<OrderDTO>(savedOrder);
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return null;

            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<List<OrderDTO>> GetMyOrdersAsync(string userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return _mapper.Map<List<OrderDTO>>(orders);
        }

        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return _mapper.Map<List<OrderDTO>>(orders);
        }

        public async Task<bool> UpdateStatusAsync(int id, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;

            order.Status = status;
            await _orderRepository.UpdateAsync(order);
            return true;
        }
    }
}