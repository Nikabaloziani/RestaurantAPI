namespace RestaurantDATA.Entities;

public class SubscriptionPlan
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPriceUsd { get; set; }
    public int MaxLocations { get; set; }
    public int MaxEmployees { get; set; }
    public bool ReservationsEnabled { get; set; } = true;
    public bool BasicAnalyticsEnabled { get; set; } = true;
    public bool AdvancedAnalyticsEnabled { get; set; }
    public bool QrMenuEnabled { get; set; }
    public bool KitchenDisplayEnabled { get; set; }
    public bool InventoryEnabled { get; set; }
    public bool MultiBranchAnalyticsEnabled { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
