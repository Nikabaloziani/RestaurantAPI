using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Configurations;

public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code).HasMaxLength(30);
        builder.Property(x => x.Name).HasMaxLength(70).IsRequired();
        builder.Property(x => x.MonthlyPriceUsd).HasPrecision(10, 2);

        builder.HasData(
            new SubscriptionPlan
            {
                Code = "starter", Name = "Starter", MonthlyPriceUsd = 39.99m,
                MaxLocations = 1, MaxEmployees = 10,
                ReservationsEnabled = true, BasicAnalyticsEnabled = true
            },
            new SubscriptionPlan
            {
                Code = "pro", Name = "Pro", MonthlyPriceUsd = 79.99m,
                MaxLocations = 2, MaxEmployees = 20,
                ReservationsEnabled = true, BasicAnalyticsEnabled = true,
                AdvancedAnalyticsEnabled = true, QrMenuEnabled = true,
                KitchenDisplayEnabled = true
            },
            new SubscriptionPlan
            {
                Code = "business", Name = "Business", MonthlyPriceUsd = 179.00m,
                MaxLocations = 5, MaxEmployees = 60,
                ReservationsEnabled = true, BasicAnalyticsEnabled = true,
                AdvancedAnalyticsEnabled = true, QrMenuEnabled = true,
                KitchenDisplayEnabled = true, InventoryEnabled = true,
                MultiBranchAnalyticsEnabled = true
            }
        );
    }
}
