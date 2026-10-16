using Microsoft.EntityFrameworkCore;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDAL.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByOrderIdAsync(int orderId);
        Task<List<Payment>> GetAllAsync();
        Task UpdateAsync(Payment payment);
    }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly RestaurantDB _context;

        public PaymentRepository(RestaurantDB context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
    }
}
