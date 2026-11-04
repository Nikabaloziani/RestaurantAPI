using RestaurantSERVICE.DTO.Billing;

namespace RestaurantSERVICE.Interfaces;

public interface IOrganizationBillingService
{
    Task<List<PlanDTO>> GetPlansAsync(CancellationToken ct);
    Task<OrganizationDTO> CreateOrganizationAsync(string userId, string name, CancellationToken ct);
    Task<List<OrganizationDTO>> GetMineAsync(string userId, CancellationToken ct);
    Task<SubscriptionDTO?> GetSubscriptionAsync(Guid organizationId, string userId, CancellationToken ct);
    Task<OrganizationDTO> AttachExistingRestaurantAsync(Guid organizationId, Guid restaurantId, string userId, CancellationToken ct);
}
