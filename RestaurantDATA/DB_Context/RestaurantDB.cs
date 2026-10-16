using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.Entities;

namespace RestaurantDATA.DB_Context
{
    public class RestaurantDB : IdentityDbContext<ApplicationUser>
    {
        public RestaurantDB(DbContextOptions<RestaurantDB> options) : base(options)
        {
        }

        public DbSet<Products> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RestaurantDB).Assembly);
        }
    }
}