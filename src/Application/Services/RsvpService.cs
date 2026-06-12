using Microsoft.EntityFrameworkCore;
using WedNest.Application.DTOs;
using WedNest.Domain.Entities;
using WedNest.Infrastructure.Data;

namespace WedNest.Application.Services;

public class RsvpService
{
    private readonly ApplicationDbContext _context;

    public RsvpService(ApplicationDbContext context) => _context = context;

    public async Task<List<GuestRsvpDto>> GetAllAsync(Guid weddingId)
    {
        return await _context.GuestRsvps
            .Where(r => r.WeddingId == weddingId)
            .OrderBy(r => r.CreatedAt)
            .Select(r => MapToDto(r))
            .ToListAsync();
    }

    public async Task<GuestRsvpDto?> GetByIdAsync(Guid weddingId, Guid id)
    {
        var rsvp = await _context.GuestRsvps
            .FirstOrDefaultAsync(r => r.WeddingId == weddingId && r.Id == id);
        return rsvp == null ? null : MapToDto(rsvp);
    }

    public async Task<GuestRsvpDto> CreateAsync(Guid weddingId, CreateRsvpRequest request)
    {
        var wedding = await _context.Weddings.FindAsync(weddingId);
        if (wedding == null) throw new InvalidOperationException("Wedding not found");

        var rsvp = new GuestRsvp
        {
            Id = Guid.NewGuid(),
            WeddingId = weddingId,
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            Status = Enum.TryParse<RsvpStatus>(request.Status, true, out var status) ? status : RsvpStatus.Attending,
            PlusOnes = request.PlusOnes,
            DietaryRestrictions = request.DietaryRestrictions,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        _context.GuestRsvps.Add(rsvp);
        await _context.SaveChangesAsync();
        return MapToDto(rsvp);
    }

    public async Task<GuestRsvpDto?> UpdateAsync(Guid weddingId, Guid id, UpdateRsvpRequest request)
    {
        var rsvp = await _context.GuestRsvps
            .FirstOrDefaultAsync(r => r.WeddingId == weddingId && r.Id == id);
        if (rsvp == null) return null;

        if (request.GuestName != null) rsvp.GuestName = request.GuestName;
        if (request.GuestEmail != null) rsvp.GuestEmail = request.GuestEmail;
        if (request.Status != null && Enum.TryParse<RsvpStatus>(request.Status, true, out var status))
            rsvp.Status = status;
        if (request.PlusOnes.HasValue) rsvp.PlusOnes = request.PlusOnes.Value;
        if (request.DietaryRestrictions != null) rsvp.DietaryRestrictions = request.DietaryRestrictions;
        if (request.Message != null) rsvp.Message = request.Message;

        rsvp.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapToDto(rsvp);
    }

    public async Task<bool> DeleteAsync(Guid weddingId, Guid id)
    {
        var rsvp = await _context.GuestRsvps
            .FirstOrDefaultAsync(r => r.WeddingId == weddingId && r.Id == id);
        if (rsvp == null) return false;

        _context.GuestRsvps.Remove(rsvp);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RsvpStatsDto> GetStatsAsync(Guid weddingId)
    {
        var rsvps = await _context.GuestRsvps
            .Where(r => r.WeddingId == weddingId)
            .ToListAsync();

        return new RsvpStatsDto
        {
            TotalInvited = rsvps.Count,
            Attending = rsvps.Count(r => r.Status == RsvpStatus.Attending),
            Declined = rsvps.Count(r => r.Status == RsvpStatus.Declined),
            Tentative = rsvps.Count(r => r.Status == RsvpStatus.Tentative),
            Pending = rsvps.Count(r => r.Status == RsvpStatus.Pending),
            TotalPlusOnes = rsvps.Where(r => r.Status == RsvpStatus.Attending).Sum(r => r.PlusOnes)
        };
    }

    private static GuestRsvpDto MapToDto(GuestRsvp r) => new()
    {
        Id = r.Id,
        WeddingId = r.WeddingId,
        GuestName = r.GuestName,
        GuestEmail = r.GuestEmail,
        Status = r.Status.ToString(),
        PlusOnes = r.PlusOnes,
        DietaryRestrictions = r.DietaryRestrictions,
        Message = r.Message,
        CreatedAt = r.CreatedAt
    };
}
