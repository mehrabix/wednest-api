using Microsoft.EntityFrameworkCore;
using WedNest.Application.DTOs;
using WedNest.Application.DTOs.Auth;
using WedNest.Domain.Entities;
using WedNest.Infrastructure.Data;

namespace WedNest.Application.Services;

public class LanguageService
{
    private readonly ApplicationDbContext _context;

    public LanguageService(ApplicationDbContext context) => _context = context;

    public async Task<List<LanguageDto>> GetAllAsync()
    {
        return await _context.Languages
            .OrderBy(l => l.DisplayOrder)
            .Select(l => new LanguageDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                NativeName = l.NativeName,
                IsActive = l.IsActive,
                IsDefault = l.IsDefault,
                DisplayOrder = l.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<LanguageDto?> GetByCodeAsync(string code)
    {
        var lang = await _context.Languages.FirstOrDefaultAsync(l => l.Code == code);
        if (lang == null) return null;
        return new LanguageDto
        {
            Id = lang.Id,
            Code = lang.Code,
            Name = lang.Name,
            NativeName = lang.NativeName,
            IsActive = lang.IsActive,
            IsDefault = lang.IsDefault,
            DisplayOrder = lang.DisplayOrder
        };
    }

    public async Task<LanguageDto> CreateAsync(CreateLanguageRequest request)
    {
        var lang = new Language
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            NativeName = request.NativeName,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.Languages.Add(lang);
        await _context.SaveChangesAsync();
        return new LanguageDto
        {
            Id = lang.Id,
            Code = lang.Code,
            Name = lang.Name,
            NativeName = lang.NativeName,
            IsActive = lang.IsActive,
            IsDefault = lang.IsDefault,
            DisplayOrder = lang.DisplayOrder
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var lang = await _context.Languages.FindAsync(id);
        if (lang == null) return false;
        _context.Languages.Remove(lang);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class TranslationService
{
    private readonly ApplicationDbContext _context;

    public TranslationService(ApplicationDbContext context) => _context = context;

    // Wedding Translations
    public async Task<List<WeddingTranslationDto>> GetWeddingTranslationsAsync(Guid weddingId)
    {
        return await _context.WeddingTranslations
            .Where(t => t.WeddingId == weddingId)
            .Select(t => new WeddingTranslationDto
            {
                Id = t.Id,
                WeddingId = t.WeddingId,
                LanguageId = t.LanguageId,
                LanguageCode = t.Language.Code,
                Title = t.Title,
                Description = t.Description,
                Venue = t.Venue
            })
            .ToListAsync();
    }

    public async Task<WeddingTranslationDto?> GetWeddingTranslationAsync(Guid weddingId, Guid languageId)
    {
        var t = await _context.WeddingTranslations
            .FirstOrDefaultAsync(t => t.WeddingId == weddingId && t.LanguageId == languageId);
        if (t == null) return null;
        return new WeddingTranslationDto
        {
            Id = t.Id,
            WeddingId = t.WeddingId,
            LanguageId = t.LanguageId,
            LanguageCode = t.Language.Code,
            Title = t.Title,
            Description = t.Description,
            Venue = t.Venue
        };
    }

    public async Task<WeddingTranslationDto> UpsertWeddingTranslationAsync(Guid weddingId, CreateTranslationRequest request)
    {
        var existing = await _context.WeddingTranslations
            .FirstOrDefaultAsync(t => t.WeddingId == weddingId && t.LanguageId == request.LanguageId);

        if (existing != null)
        {
            existing.Title = request.Title;
            existing.Description = request.Description;
            existing.Venue = request.Venue;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new WeddingTranslation
            {
                Id = Guid.NewGuid(),
                WeddingId = weddingId,
                LanguageId = request.LanguageId,
                Title = request.Title,
                Description = request.Description,
                Venue = request.Venue,
                CreatedAt = DateTime.UtcNow
            };
            _context.WeddingTranslations.Add(existing);
        }

        await _context.SaveChangesAsync();
        var langCode = await _context.Languages.Where(l => l.Id == request.LanguageId).Select(l => l.Code).FirstOrDefaultAsync();
        return new WeddingTranslationDto
        {
            Id = existing.Id,
            WeddingId = existing.WeddingId,
            LanguageId = existing.LanguageId,
            LanguageCode = langCode ?? "",
            Title = existing.Title,
            Description = existing.Description,
            Venue = existing.Venue
        };
    }

    // GiftItem Translations
    public async Task<List<GiftItemTranslationDto>> GetGiftItemTranslationsAsync(Guid giftItemId)
    {
        return await _context.GiftItemTranslations
            .Where(t => t.GiftItemId == giftItemId)
            .Select(t => new GiftItemTranslationDto
            {
                Id = t.Id,
                GiftItemId = t.GiftItemId,
                LanguageId = t.LanguageId,
                LanguageCode = t.Language.Code,
                Name = t.Name,
                Description = t.Description
            })
            .ToListAsync();
    }

    public async Task<GiftItemTranslationDto> UpsertGiftItemTranslationAsync(Guid giftItemId, Guid languageId, string name, string? description)
    {
        var existing = await _context.GiftItemTranslations
            .FirstOrDefaultAsync(t => t.GiftItemId == giftItemId && t.LanguageId == languageId);

        if (existing != null)
        {
            existing.Name = name;
            existing.Description = description;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new GiftItemTranslation
            {
                Id = Guid.NewGuid(),
                GiftItemId = giftItemId,
                LanguageId = languageId,
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _context.GiftItemTranslations.Add(existing);
        }

        await _context.SaveChangesAsync();
        var langCode = await _context.Languages.Where(l => l.Id == languageId).Select(l => l.Code).FirstOrDefaultAsync();
        return new GiftItemTranslationDto
        {
            Id = existing.Id,
            GiftItemId = existing.GiftItemId,
            LanguageId = existing.LanguageId,
            LanguageCode = langCode ?? "",
            Name = existing.Name,
            Description = existing.Description
        };
    }

    // CashFund Translations
    public async Task<List<CashFundTranslationDto>> GetCashFundTranslationsAsync(Guid cashFundId)
    {
        return await _context.CashFundTranslations
            .Where(t => t.CashFundId == cashFundId)
            .Select(t => new CashFundTranslationDto
            {
                Id = t.Id,
                CashFundId = t.CashFundId,
                LanguageId = t.LanguageId,
                LanguageCode = t.Language.Code,
                Name = t.Name,
                Description = t.Description
            })
            .ToListAsync();
    }

    public async Task<CashFundTranslationDto> UpsertCashFundTranslationAsync(Guid cashFundId, Guid languageId, string name, string? description)
    {
        var existing = await _context.CashFundTranslations
            .FirstOrDefaultAsync(t => t.CashFundId == cashFundId && t.LanguageId == languageId);

        if (existing != null)
        {
            existing.Name = name;
            existing.Description = description;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new CashFundTranslation
            {
                Id = Guid.NewGuid(),
                CashFundId = cashFundId,
                LanguageId = languageId,
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _context.CashFundTranslations.Add(existing);
        }

        await _context.SaveChangesAsync();
        var langCode = await _context.Languages.Where(l => l.Id == languageId).Select(l => l.Code).FirstOrDefaultAsync();
        return new CashFundTranslationDto
        {
            Id = existing.Id,
            CashFundId = existing.CashFundId,
            LanguageId = existing.LanguageId,
            LanguageCode = langCode ?? "",
            Name = existing.Name,
            Description = existing.Description
        };
    }
}

public class PublicService
{
    private readonly ApplicationDbContext _context;

    public PublicService(ApplicationDbContext context) => _context = context;

    public async Task<PublicWeddingDto?> GetWeddingBySlugAsync(string slug, string? langCode = null)
    {
        var wedding = await _context.Weddings
            .FirstOrDefaultAsync(w => w.Slug == slug && w.IsPublic);
        if (wedding == null) return null;

        // Try to get translation
        string title = wedding.Title;
        string? description = wedding.Description;
        string? venue = wedding.Venue;
        string language = langCode ?? "en";

        if (!string.IsNullOrEmpty(langCode))
        {
            var lang = await _context.Languages.FirstOrDefaultAsync(l => l.Code == langCode);
            if (lang != null)
            {
                var translation = await _context.WeddingTranslations
                    .FirstOrDefaultAsync(t => t.WeddingId == wedding.Id && t.LanguageId == lang.Id);
                if (translation != null)
                {
                    title = translation.Title;
                    description = translation.Description;
                    venue = translation.Venue;
                    language = langCode;
                }
            }
        }

        var giftItems = await _context.GiftItems
            .Where(g => g.WeddingId == wedding.Id)
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new PublicGiftItemDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Price = g.Price,
                ImageUrl = g.ImageUrl,
                StoreUrl = g.StoreUrl,
                Status = g.Status.ToString(),
                Quantity = g.Quantity,
                QuantityPurchased = g.QuantityPurchased
            })
            .ToListAsync();

        var cashFunds = await _context.CashFunds
            .Where(c => c.WeddingId == wedding.Id)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new PublicCashFundDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                TargetAmount = c.TargetAmount,
                CurrentAmount = c.CurrentAmount,
                ImageUrl = c.ImageUrl
            })
            .ToListAsync();

        return new PublicWeddingDto
        {
            Id = wedding.Id,
            Title = title,
            WeddingDate = wedding.WeddingDate,
            Venue = venue,
            Description = description,
            CoverImageUrl = wedding.CoverImageUrl,
            Slug = wedding.Slug,
            Language = language,
            GiftItems = giftItems,
            CashFunds = cashFunds
        };
    }
}

public class UserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context) => _context = context;

    public async Task<UserDto?> GetByKeycloakIdAsync(string keycloakId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
        if (user == null) return null;
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = (int)user.Role
        };
    }

    public async Task<UserDto?> UpdateAsync(string keycloakId, UserUpdateRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
        if (user == null) return null;

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = (int)user.Role
        };
    }
}
