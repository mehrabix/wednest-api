namespace WedNest.Domain.Entities;

public class CashFundTranslation : BaseEntity
{
    public Guid CashFundId { get; set; }
    public CashFund CashFund { get; set; } = null!;

    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
