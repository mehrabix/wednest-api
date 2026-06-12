using Microsoft.EntityFrameworkCore;
using WedNest.Domain.Entities;

namespace WedNest.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Wedding> Weddings => Set<Wedding>();
    public DbSet<GiftItem> GiftItems => Set<GiftItem>();
    public DbSet<CashFund> CashFunds => Set<CashFund>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
        });

        modelBuilder.Entity<Wedding>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Venue).HasMaxLength(300);

            entity.HasOne(e => e.Partner1)
                .WithMany(u => u.WeddingsAsPartner1)
                .HasForeignKey(e => e.Partner1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Partner2)
                .WithMany(u => u.WeddingsAsPartner2)
                .HasForeignKey(e => e.Partner2Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GiftItem>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.StoreUrl).HasMaxLength(500);

            entity.HasOne(e => e.Wedding)
                .WithMany(w => w.GiftItems)
                .HasForeignKey(e => e.WeddingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CashFund>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.TargetAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CurrentAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Wedding)
                .WithMany(w => w.CashFunds)
                .HasForeignKey(e => e.WeddingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.GuestName).HasMaxLength(200);
            entity.Property(e => e.GuestEmail).HasMaxLength(256);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Wedding)
                .WithMany()
                .HasForeignKey(e => e.WeddingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashFund)
                .WithMany(cf => cf.Orders)
                .HasForeignKey(e => e.CashFundId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.GiftItem)
                .WithMany(g => g.OrderItems)
                .HasForeignKey(e => e.GiftItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.StripePaymentIntentId).HasMaxLength(256);
            entity.Property(e => e.StripeSessionId).HasMaxLength(256);

            entity.HasIndex(e => e.StripePaymentIntentId).IsUnique();
            entity.HasIndex(e => e.StripeSessionId);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
