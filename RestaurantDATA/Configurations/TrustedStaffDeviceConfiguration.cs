using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Configurations;

public sealed class TrustedStaffDeviceConfiguration : IEntityTypeConfiguration<TrustedStaffDevice>
{
    public void Configure(EntityTypeBuilder<TrustedStaffDevice> builder)
    {
        builder.ToTable("TrustedStaffDevices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CredentialHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EnrolledByUserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(x => x.CredentialHash).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.RestaurantId });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Restaurant>()
            .WithMany()
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.EnrolledByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
