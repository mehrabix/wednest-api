namespace WedNest.Domain.Entities;

public enum GiftItemStatus
{
    Available,
    Reserved,
    Purchased
}

public class GiftItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? StoreUrl { get; set; }
    public GiftItemStatus Status { get; set; } = GiftItemStatus.Available;
    public int Quantity { get; set; } = 1;
    public int QuantityPurchased { get; set; } = 0;
    public int DisplayOrder { get; set; }

    public Guid WeddingId { get; set; }
    public Wedding Wedding { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
