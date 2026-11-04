using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Configurations;

public sealed class StaffInvitationConfiguration : IEntityTypeConfiguration<StaffInvitation>
{
    public void Configure(EntityTypeBuilder<StaffInvitation> builder)
    {
        builder.ToTable("StaffInvitations");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Email).HasMaxLength(256).IsRequired();
        builder.Property(i => i.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(i => i.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(i => i.CreatedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(i => i.AcceptedByUserId).HasMaxLength(450);
        builder.HasIndex(i => i.TokenHash).IsUnique();
        builder.HasIndex(i => new { i.RestaurantId, i.NormalizedEmail });
        builder.HasOne(i => i.Restaurant)
            .WithMany()
            .HasForeignKey(i => i.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
