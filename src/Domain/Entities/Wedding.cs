namespace WedNest.Domain.Entities;

public enum WeddingStatus
{
    Draft,
    Active,
    Completed,
    Cancelled
}

public class Wedding : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public DateTime WeddingDate { get; set; }
    public string? Venue { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Slug { get; set; } = string.Empty;
    public WeddingStatus Status { get; set; } = WeddingStatus.Draft;
    public bool IsPublic { get; set; } = true;

    public Guid Partner1Id { get; set; }
    public Guid Partner2Id { get; set; }

    public User Partner1 { get; set; } = null!;
    public User Partner2 { get; set; } = null!;

    public ICollection<GiftItem> GiftItems { get; set; } = new List<GiftItem>();
    public ICollection<CashFund> CashFunds { get; set; } = new List<CashFund>();
    public ICollection<WeddingTranslation> Translations { get; set; } = new List<WeddingTranslation>();
}
