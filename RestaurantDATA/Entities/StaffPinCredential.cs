using System;

namespace RestaurantDATA.Entities;

public sealed class StaffPinCredential
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public string PinHash { get; set; } = string.Empty;
    public int PepperKeyVersion { get; set; } = 1;
    public bool IsEnabled { get; set; } = true;
    public int FailedAttemptCount { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
