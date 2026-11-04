using RestaurantDATA.Enum;

namespace RestaurantSERVICE.DTO.Staff;

public sealed class InviteStaffDTO
{
    public string Email { get; set; } = string.Empty;
    public RestaurantRole Role { get; set; }
}

public sealed class AcceptStaffInvitationDTO
{
    public string Token { get; set; } = string.Empty;
}

public sealed class UpdateStaffRoleDTO
{
    public RestaurantRole Role { get; set; }
}

public sealed class StaffMemberDTO
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public RestaurantRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class StaffInvitationDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public RestaurantRole Role { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

// Token is a secret and is returned only on invitation creation.
public sealed class CreatedStaffInvitationDTO : StaffInvitationDTO
{
    public string Token { get; set; } = string.Empty;
}

public sealed class StaffOverviewDTO
{
    public Guid RestaurantId { get; set; }
    public Guid OrganizationId { get; set; }
    public int UsedEmployees { get; set; }
    public int MaxEmployees { get; set; }
    public List<StaffMemberDTO> Members { get; set; } = new();
}
