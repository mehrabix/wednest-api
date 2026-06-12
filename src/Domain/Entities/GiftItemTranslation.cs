namespace WedNest.Domain.Entities;

public class GiftItemTranslation : BaseEntity
{
    public Guid GiftItemId { get; set; }
    public GiftItem GiftItem { get; set; } = null!;

    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
