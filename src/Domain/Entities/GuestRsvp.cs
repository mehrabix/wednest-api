namespace WedNest.Domain.Entities;

public enum RsvpStatus
{
    Pending,
    Attending,
    Declined,
    Tentative
}

public class GuestRsvp : BaseEntity
{
    public Guid WeddingId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public RsvpStatus Status { get; set; } = RsvpStatus.Pending;
    public int PlusOnes { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? Message { get; set; }

    public Wedding Wedding { get; set; } = null!;
}
