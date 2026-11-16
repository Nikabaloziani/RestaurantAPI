using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantSERVICE.DTO.StaffPin;

// This is account-password confirmation, NOT a PIN-login endpoint.
public sealed class SetMyPinDTO
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    public string NewPin { get; set; } = string.Empty;
}

public sealed class DisableMyPinDTO
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
}

public sealed class MyPinStatusDTO
{
    public bool HasPin { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
