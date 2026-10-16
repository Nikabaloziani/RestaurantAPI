using AutoMapper;
using RestaurantDAL.Repositories;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Payment;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<PaymentDTO> ProcessPaymentAsync(CreatePaymentDTO dto)
        {
            var order = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (order == null)
            {
                throw new ArgumentException("Order not found.");
            }

            if (dto.Amount < order.TotalAmount)
            {
                throw new InvalidOperationException("Payment amount is less than the order total amount.");
            }

            var payment = new Payment
            {
                OrderId = dto.OrderId,
                Amount = dto.Amount,
                Method = dto.Method,
                Status = PaymentStatus.Completed,
                PaymentDate = DateTime.UtcNow,
                TransactionId = Guid.NewGuid().ToString() 
            };

            await _paymentRepository.AddAsync(payment);

            order.Status = OrderStatus.Paid;
            await _orderRepository.UpdateAsync(order);

            var createdPayment = await _paymentRepository.GetByIdAsync(payment.Id);
            return _mapper.Map<PaymentDTO>(createdPayment);
        }

        public async Task<PaymentDTO?> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            return payment == null ? null : _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<PaymentDTO?> GetPaymentByOrderIdAsync(int orderId)
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
            return payment == null ? null : _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<List<PaymentDTO>> GetAllPaymentsAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return _mapper.Map<List<PaymentDTO>>(payments);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, PaymentStatus status)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null) return false;

            payment.Status = status;
            await _paymentRepository.UpdateAsync(payment);
            return true;
        }
    }
}