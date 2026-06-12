using Microsoft.EntityFrameworkCore;
using WedNest.Application.DTOs;
using WedNest.Domain.Entities;
using WedNest.Infrastructure.Data;

namespace WedNest.Application.Services;

public class GiftItemService
{
    private readonly ApplicationDbContext _context;

    public GiftItemService(ApplicationDbContext context) => _context = context;

    public async Task<List<GiftItemDto>> GetByWeddingAsync(Guid weddingId)
    {
        return await _context.GiftItems
            .Where(g => g.WeddingId == weddingId)
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new GiftItemDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Price = g.Price,
                ImageUrl = g.ImageUrl,
                StoreUrl = g.StoreUrl,
                Status = g.Status.ToString(),
                Quantity = g.Quantity,
                QuantityPurchased = g.QuantityPurchased,
                DisplayOrder = g.DisplayOrder,
                WeddingId = g.WeddingId
            })
            .ToListAsync();
    }

    public async Task<GiftItemDto?> GetByIdAsync(Guid id)
    {
        var item = await _context.GiftItems.FindAsync(id);
        if (item == null) return null;
        return new GiftItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            ImageUrl = item.ImageUrl,
            StoreUrl = item.StoreUrl,
            Status = item.Status.ToString(),
            Quantity = item.Quantity,
            QuantityPurchased = item.QuantityPurchased,
            DisplayOrder = item.DisplayOrder,
            WeddingId = item.WeddingId
        };
    }

    public async Task<GiftItemDto> CreateAsync(Guid weddingId, CreateGiftItemRequest request)
    {
        var item = new GiftItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            StoreUrl = request.StoreUrl,
            Quantity = request.Quantity,
            DisplayOrder = request.DisplayOrder,
            WeddingId = weddingId,
            Status = GiftItemStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        _context.GiftItems.Add(item);
        await _context.SaveChangesAsync();
        return new GiftItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            ImageUrl = item.ImageUrl,
            StoreUrl = item.StoreUrl,
            Status = item.Status.ToString(),
            Quantity = item.Quantity,
            QuantityPurchased = item.QuantityPurchased,
            DisplayOrder = item.DisplayOrder,
            WeddingId = item.WeddingId
        };
    }

    public async Task<GiftItemDto?> UpdateAsync(Guid id, UpdateGiftItemRequest request)
    {
        var item = await _context.GiftItems.FindAsync(id);
        if (item == null) return null;

        if (request.Name != null) item.Name = request.Name;
        if (request.Description != null) item.Description = request.Description;
        if (request.Price.HasValue) item.Price = request.Price.Value;
        if (request.ImageUrl != null) item.ImageUrl = request.ImageUrl;
        if (request.StoreUrl != null) item.StoreUrl = request.StoreUrl;
        if (request.Quantity.HasValue) item.Quantity = request.Quantity.Value;
        if (request.DisplayOrder.HasValue) item.DisplayOrder = request.DisplayOrder.Value;
        if (request.Status != null && Enum.TryParse<GiftItemStatus>(request.Status, out var status))
            item.Status = status;

        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return new GiftItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            ImageUrl = item.ImageUrl,
            StoreUrl = item.StoreUrl,
            Status = item.Status.ToString(),
            Quantity = item.Quantity,
            QuantityPurchased = item.QuantityPurchased,
            DisplayOrder = item.DisplayOrder,
            WeddingId = item.WeddingId
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await _context.GiftItems.FindAsync(id);
        if (item == null) return false;
        _context.GiftItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class CashFundService
{
    private readonly ApplicationDbContext _context;

    public CashFundService(ApplicationDbContext context) => _context = context;

    public async Task<List<CashFundDto>> GetByWeddingAsync(Guid weddingId)
    {
        return await _context.CashFunds
            .Where(c => c.WeddingId == weddingId)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CashFundDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                TargetAmount = c.TargetAmount,
                CurrentAmount = c.CurrentAmount,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                WeddingId = c.WeddingId
            })
            .ToListAsync();
    }

    public async Task<CashFundDto> CreateAsync(Guid weddingId, CreateCashFundRequest request)
    {
        var fund = new CashFund
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            TargetAmount = request.TargetAmount,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            WeddingId = weddingId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CashFunds.Add(fund);
        await _context.SaveChangesAsync();
        return new CashFundDto
        {
            Id = fund.Id,
            Name = fund.Name,
            Description = fund.Description,
            TargetAmount = fund.TargetAmount,
            CurrentAmount = fund.CurrentAmount,
            ImageUrl = fund.ImageUrl,
            DisplayOrder = fund.DisplayOrder,
            WeddingId = fund.WeddingId
        };
    }

    public async Task<CashFundDto?> UpdateAsync(Guid id, UpdateCashFundRequest request)
    {
        var fund = await _context.CashFunds.FindAsync(id);
        if (fund == null) return null;

        if (request.Name != null) fund.Name = request.Name;
        if (request.Description != null) fund.Description = request.Description;
        if (request.TargetAmount.HasValue) fund.TargetAmount = request.TargetAmount;
        if (request.ImageUrl != null) fund.ImageUrl = request.ImageUrl;
        if (request.DisplayOrder.HasValue) fund.DisplayOrder = request.DisplayOrder.Value;

        fund.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return new CashFundDto
        {
            Id = fund.Id,
            Name = fund.Name,
            Description = fund.Description,
            TargetAmount = fund.TargetAmount,
            CurrentAmount = fund.CurrentAmount,
            ImageUrl = fund.ImageUrl,
            DisplayOrder = fund.DisplayOrder,
            WeddingId = fund.WeddingId
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var fund = await _context.CashFunds.FindAsync(id);
        if (fund == null) return false;
        _context.CashFunds.Remove(fund);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class OrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context) => _context = context;

    public async Task<List<OrderDto>> GetByWeddingAsync(Guid weddingId)
    {
        return await _context.Orders
            .Where(o => o.WeddingId == weddingId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                GuestName = o.GuestName,
                GuestEmail = o.GuestEmail,
                GuestMessage = o.GuestMessage,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                WeddingId = o.WeddingId,
                CashFundId = o.CashFundId,
                UserId = o.UserId
            })
            .ToListAsync();
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.GiftItem)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return null;

        return new OrderDto
        {
            Id = order.Id,
            GuestName = order.GuestName,
            GuestEmail = order.GuestEmail,
            GuestMessage = order.GuestMessage,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            WeddingId = order.WeddingId,
            CashFundId = order.CashFundId,
            UserId = order.UserId,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                GiftItemId = oi.GiftItemId,
                GiftItemName = oi.GiftItem?.Name ?? "",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, Guid userId)
    {
        decimal total = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var giftItem = await _context.GiftItems.FindAsync(item.GiftItemId);
            if (giftItem == null) continue;

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                GiftItemId = item.GiftItemId,
                Quantity = item.Quantity,
                UnitPrice = giftItem.Price,
                TotalPrice = giftItem.Price * item.Quantity
            };
            total += orderItem.TotalPrice;
            orderItems.Add(orderItem);
        }

        if (request.CashFundId.HasValue)
        {
            var fund = await _context.CashFunds.FindAsync(request.CashFundId.Value);
            if (fund != null) total += fund.TargetAmount ?? 0;
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            GuestMessage = request.GuestMessage,
            TotalAmount = total,
            Status = OrderStatus.Pending,
            WeddingId = request.WeddingId,
            CashFundId = request.CashFundId,
            UserId = userId,
            OrderItems = orderItems,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(order.Id) ?? throw new Exception("Failed to create order");
    }

    public async Task<OrderDto?> UpdateStatusAsync(Guid id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return null;

        if (Enum.TryParse<OrderStatus>(status, out var newStatus))
            order.Status = newStatus;

        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }
}
