using System.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantSERVICE.DTO.Payment;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE
{
    public class PaymentService : IPaymentService
    {
        private readonly RestaurantDB _context;
        private readonly IMapper _mapper;

        public PaymentService(RestaurantDB context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentDTO> ProcessPaymentAsync(CreatePaymentDTO dto)
        {
            if (dto.Method != PaymentMethod.Cash && dto.Method != PaymentMethod.Card)
                throw new InvalidOperationException(
                    "Only manually confirmed Cash or Card payments are supported. Online checkout is not connected.");

            if (dto.Amount <= 0)
                throw new InvalidOperationException("Payment amount must be greater than zero.");

            // Lock the order row so simultaneous payment requests cannot both pass
            // the duplicate-payment check. Insert payment + mark Paid atomically.
            // PostgreSQL is required (this project's configured database provider).
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            var rows = await _context.Orders
                .FromSqlInterpolated($"SELECT * FROM \"Orders\" WHERE \"Id\" = {dto.OrderId} FOR UPDATE")
                .ToListAsync();
            var order = rows.SingleOrDefault();

            if (order == null)
                throw new ArgumentException("Order not found.");
            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cancelled orders cannot be paid.");
            if (order.Status == OrderStatus.Paid)
                throw new InvalidOperationException("Order is already marked as paid. Review its payment history.");
            if (order.Status != OrderStatus.Pending &&
                order.Status != OrderStatus.Preparing &&
                order.Status != OrderStatus.Ready &&
                order.Status != OrderStatus.Served)
                throw new InvalidOperationException("Unsupported order status for payment.");
            if (dto.Amount != order.TotalAmount)
                throw new InvalidOperationException("Payment amount must exactly match the order total.");
            if (await _context.Payments.AnyAsync(p => p.OrderId == dto.OrderId &&
                                                      p.Status == PaymentStatus.Completed))
                throw new InvalidOperationException("A completed payment already exists for this order.");

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Method = dto.Method,
                Status = PaymentStatus.Completed,
                PaymentDate = DateTime.UtcNow,
                // Do not invent a bank transaction ID for manually recorded payments.
                TransactionId = null
            };
            _context.Payments.Add(payment);
            order.Status = OrderStatus.Paid;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<PaymentDTO?> GetPaymentByIdAsync(int id)
        {
            var payment = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            return payment == null ? null : _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<PaymentDTO?> GetPaymentByOrderIdAsync(int orderId)
        {
            var payment = await _context.Payments.AsNoTracking()
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();
            return payment == null ? null : _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<List<PaymentDTO>> GetAllPaymentsAsync()
        {
            var payments = await _context.Payments.AsNoTracking()
                .OrderByDescending(p => p.PaymentDate).ToListAsync();
            return _mapper.Map<List<PaymentDTO>>(payments);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, PaymentStatus status)
        {
            var existing = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return false;
            if (existing.Status == status) return true;
            // Refunds/reversals must be a dedicated audited workflow, rather
            // than toggling a status without adjusting the associated order.
            throw new InvalidOperationException(
                "Payment status cannot be changed manually. Refunds are not implemented.");
        }
    }
}
