namespace WedNest.Domain.Entities;

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Refunded
}

public class Payment : BaseEntity
{
    public string Authority { get; set; } = string.Empty;
    public string? RefId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public string? FailureReason { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
