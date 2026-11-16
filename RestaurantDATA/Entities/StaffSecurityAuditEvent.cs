using System;

namespace RestaurantDATA.Entities;

/// <summary>
/// Minimal security audit event. Never put a PIN, access token, device secret,
/// or refresh token in Action or Outcome.
/// </summary>
public sealed class StaffSecurityAuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid? RestaurantId { get; set; }
    public Guid? TrustedStaffDeviceId { get; set; }
    public string? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
