namespace WedNest.Application.DTOs;

public class GiftItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? StoreUrl { get; set; }
    public string Status { get; set; } = "Available";
    public int Quantity { get; set; }
    public int QuantityPurchased { get; set; }
    public int DisplayOrder { get; set; }
    public Guid WeddingId { get; set; }
}

public class CreateGiftItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? StoreUrl { get; set; }
    public int Quantity { get; set; } = 1;
    public int DisplayOrder { get; set; }
}

public class UpdateGiftItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? StoreUrl { get; set; }
    public string? Status { get; set; }
    public int? Quantity { get; set; }
    public int? DisplayOrder { get; set; }
}

public class CashFundDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public Guid WeddingId { get; set; }
}

public class CreateCashFundRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TargetAmount { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateCashFundRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? TargetAmount { get; set; }
    public string? ImageUrl { get; set; }
    public int? DisplayOrder { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string? GuestMessage { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public Guid WeddingId { get; set; }
    public Guid? CashFundId { get; set; }
    public Guid UserId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid GiftItemId { get; set; }
    public string GiftItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOrderRequest
{
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string? GuestMessage { get; set; }
    public Guid WeddingId { get; set; }
    public Guid? CashFundId { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    public Guid GiftItemId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Status { get; set; } = "Pending";
    public DateTime? PaidAt { get; set; }
    public Guid OrderId { get; set; }
}
