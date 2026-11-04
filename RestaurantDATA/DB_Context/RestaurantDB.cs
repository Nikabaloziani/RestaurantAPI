using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantDATA.Entities;

namespace RestaurantDATA.DB_Context
{
    public class RestaurantDB : IdentityDbContext<ApplicationUser>
    {
        public RestaurantDB(DbContextOptions<RestaurantDB> options) : base(options) { }

        public DbSet<Products> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<RestaurantUser> RestaurantUsers => Set<RestaurantUser>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        // NEW
        public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RestaurantDB).Assembly);
        }
    }
}
