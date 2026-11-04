using RestaurantDATA.Enum;

namespace RestaurantSERVICE.DTO.Billing;

public sealed class CreateOrganizationDTO
{
    public string Name { get; set; } = string.Empty;
}

public sealed class PlanDTO
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPriceUsd { get; set; }
    public int MaxLocations { get; set; }
    public int MaxEmployees { get; set; }
    public bool ReservationsEnabled { get; set; }
    public bool BasicAnalyticsEnabled { get; set; }
    public bool AdvancedAnalyticsEnabled { get; set; }
    public bool QrMenuEnabled { get; set; }
    public bool KitchenDisplayEnabled { get; set; }
    public bool InventoryEnabled { get; set; }
    public bool MultiBranchAnalyticsEnabled { get; set; }
}

public sealed class OrganizationDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int LocationCount { get; set; }
}

public sealed class SubscriptionDTO
{
    public Guid OrganizationId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; }
    public DateTimeOffset? TrialEndsAt { get; set; }
    public DateTimeOffset? CurrentPeriodEndsAt { get; set; }
    public int MaxLocations { get; set; }
    public int UsedLocations { get; set; }
    public int MaxEmployees { get; set; }
    public int UsedEmployees { get; set; }
    public bool IsCurrentlyEntitled { get; set; }
}
