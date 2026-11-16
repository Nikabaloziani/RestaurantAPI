using System;

namespace RestaurantDATA.Entities;

/// <summary>
/// Tracks a revocable staff session on a single trusted device and restaurant.
/// RefreshTokenHash stores only a digest of a high-entropy refresh token.
/// </summary>
public sealed class StaffDeviceSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid TrustedStaffDeviceId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string RefreshTokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
