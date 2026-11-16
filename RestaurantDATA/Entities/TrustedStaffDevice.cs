using System;

namespace RestaurantDATA.Entities;

/// <summary>
/// A restaurant device enrolled by an authorized owner.
/// CredentialHash is SHA-256 of a cryptographically random, high-entropy
/// device secret, NOT a hash of an employee PIN.
/// </summary>
public sealed class TrustedStaffDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CredentialHash { get; set; } = string.Empty;
    public string EnrolledByUserId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
