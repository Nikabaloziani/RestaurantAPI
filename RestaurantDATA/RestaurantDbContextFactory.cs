using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RestaurantDATA.DB_Context;

namespace RestaurantDATA
{
    public class RestaurantDbContextFactory : IDesignTimeDbContextFactory<RestaurantDB>
    {
        public RestaurantDB CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RestaurantDB>();

            optionsBuilder.UseNpgsql("Host=dpg-db94un3rjlhs73cafetg-a;Port=5432;Database=restaurant_db_ozcb;Username=restaurant_db_ozcb_user;Password=SQ1BG8CnvOPmFZ17PDtnGKeXM8DQ1qKv");

            return new RestaurantDB(optionsBuilder.Options);
        }
    }
}