namespace WedNest.Domain.Entities;

public enum UserRole
{
    Couple,
    Guest
}

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.Guest;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public ICollection<Wedding> WeddingsAsPartner1 { get; set; } = new List<Wedding>();
    public ICollection<Wedding> WeddingsAsPartner2 { get; set; } = new List<Wedding>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
