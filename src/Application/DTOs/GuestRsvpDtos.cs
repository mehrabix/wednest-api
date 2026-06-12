namespace WedNest.Application.DTOs;

public class GuestRsvpDto
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public string Status { get; set; } = "Pending";
    public int PlusOnes { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRsvpRequest
{
    public string GuestName { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public string Status { get; set; } = "Attending";
    public int PlusOnes { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? Message { get; set; }
}

public class UpdateRsvpRequest
{
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string? Status { get; set; }
    public int? PlusOnes { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? Message { get; set; }
}

public class RsvpStatsDto
{
    public int TotalInvited { get; set; }
    public int Attending { get; set; }
    public int Declined { get; set; }
    public int Tentative { get; set; }
    public int Pending { get; set; }
    public int TotalPlusOnes { get; set; }
}
