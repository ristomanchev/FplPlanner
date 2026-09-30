using Microsoft.EntityFrameworkCore;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Repository.Converters;

namespace ProektIntegrirani.Repository;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Club> Clubs { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Gameweek> Gameweeks { get; set; }
    public DbSet<Fixture> Fixtures { get; set; }
    public DbSet<Manager> Managers { get; set; }
    public DbSet<SquadPick> SquadPicks { get; set; }
    public DbSet<PlayerPrediction> PlayerPredictions { get; set; }
    public DbSet<ApiClient> ApiClients { get; set; }
    public DbSet<InboundSquadEntry> InboundSquadEntries { get; set; }
    public DbSet<EtlSyncLog> EtlSyncLogs { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite has no native decimal type: EF stores it as TEXT and cannot ORDER BY or SUM it.
        configurationBuilder.Properties<decimal>().HaveConversion<double>();
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Club>(entity =>
        {
            entity.HasIndex(c => c.FplId).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(100);
            entity.Property(c => c.ShortName).HasMaxLength(3);
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasIndex(p => p.FplId).IsUnique();
            entity.Property(p => p.Position).HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

            // Owned type: stats live in the Players table as Stats_* columns.
            entity.OwnsOne(p => p.Stats);

            entity.HasOne(p => p.Club)
                .WithMany(c => c.Players)
                .HasForeignKey(p => p.ClubId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Gameweek>(entity =>
        {
            entity.HasIndex(g => g.Number).IsUnique();
        });

        modelBuilder.Entity<Fixture>(entity =>
        {
            entity.HasIndex(f => f.FplId).IsUnique();

            entity.HasOne(f => f.Gameweek)
                .WithMany(g => g.Fixtures)
                .HasForeignKey(f => f.GameweekId)
                .OnDelete(DeleteBehavior.SetNull);

            // Two FKs to the same table: cascade must be disabled on both.
            entity.HasOne(f => f.HomeClub)
                .WithMany(c => c.HomeFixtures)
                .HasForeignKey(f => f.HomeClubId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.AwayClub)
                .WithMany(c => c.AwayFixtures)
                .HasForeignKey(f => f.AwayClubId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasIndex(m => m.FplEntryId).IsUnique();
        });

        modelBuilder.Entity<SquadPick>(entity =>
        {
            entity.HasIndex(sp => new { sp.ManagerId, sp.GameweekId, sp.PlayerId }).IsUnique();

            entity.HasOne(sp => sp.Manager)
                .WithMany(m => m.SquadPicks)
                .HasForeignKey(sp => sp.ManagerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sp => sp.Gameweek)
                .WithMany(g => g.SquadPicks)
                .HasForeignKey(sp => sp.GameweekId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sp => sp.Player)
                .WithMany(p => p.SquadPicks)
                .HasForeignKey(sp => sp.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlayerPrediction>(entity =>
        {
            entity.HasIndex(pp => new { pp.PlayerId, pp.GameweekId, pp.ModelType }).IsUnique();
            entity.Property(pp => pp.ModelType).HasConversion<string>().HasMaxLength(20);
            entity.OwnsOne(pp => pp.Breakdown, breakdown => breakdown.Ignore(b => b.Total));

            entity.HasOne(pp => pp.Player)
                .WithMany(p => p.Predictions)
                .HasForeignKey(pp => pp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pp => pp.Gameweek)
                .WithMany(g => g.Predictions)
                .HasForeignKey(pp => pp.GameweekId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiClient>(entity =>
        {
            entity.HasIndex(c => c.ApiKeyHash).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(100);
            entity.Property(c => c.ApiKeyHash).HasMaxLength(64);
        });

        modelBuilder.Entity<InboundSquadEntry>(entity =>
        {
            // The processor looks for Pending entries, oldest first.
            entity.HasIndex(e => new { e.Status, e.ReceivedAt });
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            // Keep the inbound log: a client with entries is deactivated, not deleted.
            entity.HasOne(e => e.ApiClient)
                .WithMany(c => c.InboundSquadEntries)
                .HasForeignKey(e => e.ApiClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
