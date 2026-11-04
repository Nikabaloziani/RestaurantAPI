using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OrganizationId).IsUnique();
        builder.Property(x => x.PlanCode).IsRequired().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.Organization)
            .WithOne(x => x.Subscription)
            .HasForeignKey<Subscription>(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Subscriptions)
            .HasForeignKey(x => x.PlanCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
