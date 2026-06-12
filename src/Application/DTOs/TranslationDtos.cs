namespace WedNest.Application.DTOs;

public class LanguageDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateLanguageRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
}

public class WeddingTranslationDto
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Venue { get; set; }
}

public class GiftItemTranslationDto
{
    public Guid Id { get; set; }
    public Guid GiftItemId { get; set; }
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CashFundTranslationDto
{
    public Guid Id { get; set; }
    public Guid CashFundId { get; set; }
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateTranslationRequest
{
    public Guid LanguageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Venue { get; set; }
}

public class PublicWeddingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime WeddingDate { get; set; }
    public string? Venue { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public List<PublicGiftItemDto> GiftItems { get; set; } = new();
    public List<PublicCashFundDto> CashFunds { get; set; } = new();
}

public class PublicGiftItemDto
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
}

public class PublicCashFundDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public string? ImageUrl { get; set; }
}

public class UserUpdateRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
