using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("Reservations");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Note)
                   .HasMaxLength(500);

            builder.Property(r => r.GuestName).HasMaxLength(100);
            builder.Property(r => r.GuestPhone).HasMaxLength(25);

            builder.HasOne(r => r.Table)
                   .WithMany()
                   .HasForeignKey(r => r.TableId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                   .WithMany()
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Restaurant)
                     .WithMany()
                     .HasForeignKey(r => r.RestaurantId)
                     .OnDelete(DeleteBehavior.Restrict);


        }

    }
}
