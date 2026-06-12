namespace WedNest.Application.DTOs;

public class WeddingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime WeddingDate { get; set; }
    public string? Venue { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public bool IsPublic { get; set; }
    public Guid Partner1Id { get; set; }
    public Guid? Partner2Id { get; set; }
    public int GiftItemCount { get; set; }
    public int CashFundCount { get; set; }
}

public class CreateWeddingRequest
{
    public string Title { get; set; } = string.Empty;
    public DateTime WeddingDate { get; set; }
    public string? Venue { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Slug { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public Guid Partner1Id { get; set; }
    public Guid? Partner2Id { get; set; }
}

public class UpdateWeddingRequest
{
    public string? Title { get; set; }
    public DateTime? WeddingDate { get; set; }
    public string? Venue { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Slug { get; set; }
    public string? Status { get; set; }
    public bool? IsPublic { get; set; }
}
