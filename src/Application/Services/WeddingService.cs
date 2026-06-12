using Microsoft.EntityFrameworkCore;
using WedNest.Application.DTOs;
using WedNest.Domain.Entities;
using WedNest.Infrastructure.Data;

namespace WedNest.Application.Services;

public class WeddingService
{
    private readonly ApplicationDbContext _context;

    public WeddingService(ApplicationDbContext context) => _context = context;

    public async Task<List<WeddingDto>> GetAllAsync()
    {
        return await _context.Weddings
            .Select(w => MapToDto(w))
            .ToListAsync();
    }

    public async Task<WeddingDto?> GetByIdAsync(Guid id)
    {
        var wedding = await _context.Weddings.FindAsync(id);
        return wedding == null ? null : MapToDto(wedding);
    }

    public async Task<WeddingDto?> GetBySlugAsync(string slug)
    {
        var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.Slug == slug);
        return wedding == null ? null : MapToDto(wedding);
    }

    public async Task<WeddingDto> CreateAsync(CreateWeddingRequest request)
    {
        var wedding = new Wedding
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            WeddingDate = request.WeddingDate,
            Venue = request.Venue,
            Description = request.Description,
            CoverImageUrl = request.CoverImageUrl,
            Slug = request.Slug,
            IsPublic = request.IsPublic,
            Partner1Id = request.Partner1Id,
            Partner2Id = request.Partner2Id != Guid.Empty ? request.Partner2Id : null,
            Status = WeddingStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _context.Weddings.Add(wedding);
        await _context.SaveChangesAsync();
        return MapToDto(wedding);
    }

    public async Task<WeddingDto?> UpdateAsync(Guid id, UpdateWeddingRequest request)
    {
        var wedding = await _context.Weddings.FindAsync(id);
        if (wedding == null) return null;

        if (request.Title != null) wedding.Title = request.Title;
        if (request.WeddingDate.HasValue) wedding.WeddingDate = request.WeddingDate.Value;
        if (request.Venue != null) wedding.Venue = request.Venue;
        if (request.Description != null) wedding.Description = request.Description;
        if (request.CoverImageUrl != null) wedding.CoverImageUrl = request.CoverImageUrl;
        if (request.Slug != null) wedding.Slug = request.Slug;
        if (request.IsPublic.HasValue) wedding.IsPublic = request.IsPublic.Value;
        if (request.Status != null && Enum.TryParse<WeddingStatus>(request.Status, out var status))
            wedding.Status = status;

        wedding.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapToDto(wedding);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var wedding = await _context.Weddings.FindAsync(id);
        if (wedding == null) return false;

        _context.Weddings.Remove(wedding);
        await _context.SaveChangesAsync();
        return true;
    }

    private static WeddingDto MapToDto(Wedding w) => new()
    {
        Id = w.Id,
        Title = w.Title,
        WeddingDate = w.WeddingDate,
        Venue = w.Venue,
        Description = w.Description,
        CoverImageUrl = w.CoverImageUrl,
        Slug = w.Slug,
        Status = w.Status.ToString(),
        IsPublic = w.IsPublic,
        Partner1Id = w.Partner1Id,
        Partner2Id = w.Partner2Id,
        GiftItemCount = w.GiftItems?.Count ?? 0,
        CashFundCount = w.CashFunds?.Count ?? 0
    };
}
