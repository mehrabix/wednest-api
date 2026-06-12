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
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<WeddingTranslation> WeddingTranslations => Set<WeddingTranslation>();
    public DbSet<GiftItemTranslation> GiftItemTranslations => Set<GiftItemTranslation>();
    public DbSet<CashFundTranslation> CashFundTranslations => Set<CashFundTranslation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure xmin concurrency token for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<uint>("xmin")
                    .HasColumnType("xid")
                    .IsRowVersion();
            }
        }

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.KeycloakId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.KeycloakId).HasMaxLength(128);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
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
            entity.Property(e => e.Authority).HasMaxLength(256);

            entity.HasIndex(e => e.Authority).IsUnique();

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.NativeName).HasMaxLength(100);
        });

        modelBuilder.Entity<WeddingTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.WeddingId, e.LanguageId }).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Venue).HasMaxLength(300);

            entity.HasOne(e => e.Wedding)
                .WithMany(w => w.Translations)
                .HasForeignKey(e => e.WeddingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                .WithMany()
                .HasForeignKey(e => e.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GiftItemTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.GiftItemId, e.LanguageId }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(e => e.GiftItem)
                .WithMany(g => g.Translations)
                .HasForeignKey(e => e.GiftItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                .WithMany()
                .HasForeignKey(e => e.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CashFundTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.CashFundId, e.LanguageId }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(e => e.CashFund)
                .WithMany(c => c.Translations)
                .HasForeignKey(e => e.CashFundId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                .WithMany()
                .HasForeignKey(e => e.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var langEnId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var langArId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var langFaId = Guid.Parse("10000000-0000-0000-0000-000000000003");

        modelBuilder.Entity<Language>().HasData(
            new Language { Id = langEnId, Code = "en", Name = "English", NativeName = "English", IsActive = true, IsDefault = true, DisplayOrder = 1, CreatedAt = now },
            new Language { Id = langArId, Code = "ar", Name = "Arabic", NativeName = "\u0627\u0644\u0639\u0631\u0628\u064a\u0629", IsActive = true, IsDefault = false, DisplayOrder = 2, CreatedAt = now },
            new Language { Id = langFaId, Code = "fa", Name = "Persian", NativeName = "\u0641\u0627\u0631\u0633\u06cc", IsActive = true, IsDefault = false, DisplayOrder = 3, CreatedAt = now }
        );

        // Users are managed by Keycloak — skip seeding them to avoid FK conflicts.
        // Wedding/gifts/cash funds depend on user IDs, so seed those via API after login.
    }
}
