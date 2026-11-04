namespace RestaurantDATA.Entities;

public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Subscription? Subscription { get; set; }
    public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
}
