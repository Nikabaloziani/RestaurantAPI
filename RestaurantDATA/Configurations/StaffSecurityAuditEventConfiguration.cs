using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Configurations;

public sealed class StaffSecurityAuditEventConfiguration : IEntityTypeConfiguration<StaffSecurityAuditEvent>
{
    public void Configure(EntityTypeBuilder<StaffSecurityAuditEvent> builder)
    {
        builder.ToTable("StaffSecurityAuditEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorUserId).HasMaxLength(450);
        builder.Property(x => x.Action).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Outcome).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => new { x.OrganizationId, x.OccurredAt });
        builder.HasIndex(x => new { x.TrustedStaffDeviceId, x.OccurredAt });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Restaurant>()
            .WithMany()
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
