namespace WedNest.Domain.Entities;

public enum OrderStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

public class Order : BaseEntity
{
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string? GuestMessage { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public Guid WeddingId { get; set; }
    public Wedding Wedding { get; set; } = null!;

    public Guid? CashFundId { get; set; }
    public CashFund? CashFund { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
