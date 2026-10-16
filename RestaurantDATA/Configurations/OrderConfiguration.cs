using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}