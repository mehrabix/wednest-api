namespace WedNest.Domain.Entities;

public enum UserRole
{
    Guest,
    User,
    Couple,
    Admin
}

public class User : BaseEntity
{
    public string KeycloakId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.Guest;

    public ICollection<Wedding> WeddingsAsPartner1 { get; set; } = new List<Wedding>();
    public ICollection<Wedding> WeddingsAsPartner2 { get; set; } = new List<Wedding>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
