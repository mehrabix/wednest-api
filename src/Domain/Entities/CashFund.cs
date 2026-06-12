namespace WedNest.Domain.Entities;

public class CashFund : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; } = 0;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }

    public Guid WeddingId { get; set; }
    public Wedding Wedding { get; set; } = null!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
