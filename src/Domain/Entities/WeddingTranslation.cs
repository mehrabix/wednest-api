namespace WedNest.Domain.Entities;

public class WeddingTranslation : BaseEntity
{
    public Guid WeddingId { get; set; }
    public Wedding Wedding { get; set; } = null!;

    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Venue { get; set; }
}
