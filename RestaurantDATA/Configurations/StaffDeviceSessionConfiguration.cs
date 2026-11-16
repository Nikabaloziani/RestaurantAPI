using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Configurations;

public sealed class StaffDeviceSessionConfiguration : IEntityTypeConfiguration<StaffDeviceSession>
{
    public void Configure(EntityTypeBuilder<StaffDeviceSession> builder)
    {
        builder.ToTable("StaffDeviceSessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.RefreshTokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.RefreshTokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.RevokedAt });
        builder.HasIndex(x => new { x.TrustedStaffDeviceId, x.ExpiresAt });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Restaurant>()
            .WithMany()
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TrustedStaffDevice>()
            .WithMany()
            .HasForeignKey(x => x.TrustedStaffDeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
