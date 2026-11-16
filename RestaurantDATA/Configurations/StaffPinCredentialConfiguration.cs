using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Configurations;

public sealed class StaffPinCredentialConfiguration : IEntityTypeConfiguration<StaffPinCredential>
{
    public void Configure(EntityTypeBuilder<StaffPinCredential> builder)
    {
        builder.ToTable("StaffPinCredentials");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.PinHash).HasMaxLength(512).IsRequired();
        builder.HasIndex(x => new { x.OrganizationId, x.UserId }).IsUnique();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
