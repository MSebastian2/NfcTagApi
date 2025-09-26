using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using DeleteBehavior = Microsoft.EntityFrameworkCore.DeleteBehavior;

namespace Clocking.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<Reader> Readers => Set<Reader>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Scan> Scans => Set<Scan>();
    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Worker
        b.Entity<Worker>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.TagUid).HasMaxLength(64);
            e.HasIndex(x => x.TagUid).IsUnique();

            e.HasMany(x => x.WorkSessions)
             .WithOne(ws => ws.Worker)
             .HasForeignKey(ws => ws.WorkerId)
             .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

            e.HasMany(x => x.Scans)
             .WithOne(s => s.Worker)
             .HasForeignKey(s => s.WorkerId)
             .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
        });

        // Reader
        b.Entity<Reader>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200);

            e.HasOne(x => x.Location)
             .WithMany(l => l.Readers)
             .HasForeignKey(x => x.LocationId)
             .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.SetNull);
        });

        // Location
        b.Entity<Location>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Code).HasMaxLength(64);
            e.HasIndex(x => x.Code);
        });

        // Scan
        b.Entity<Scan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.WhenUtc).IsRequired();   // DateTime or DateTimeOffset, your entity decides
            e.Property(x => x.Type).IsRequired();

            e.HasOne(x => x.Worker)
             .WithMany(w => w.Scans)
             .HasForeignKey(x => x.WorkerId)
             .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

            e.HasOne(x => x.Reader)
             .WithMany()
             .HasForeignKey(x => x.ReaderId)
             .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.SetNull);

            e.HasIndex(x => x.WhenUtc);
        });

        // WorkSession
        b.Entity<WorkSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StartUtc).IsRequired();   // DateTime or DateTimeOffset
            e.Property(x => x.EndUtc);                  // nullable when clocked-in

            e.HasOne(x => x.Worker)
             .WithMany(w => w.WorkSessions)
             .HasForeignKey(x => x.WorkerId)
             .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

            e.HasOne(x => x.StartReader)
             .WithMany()
             .HasForeignKey(x => x.StartReaderId)
             .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.SetNull);

            e.HasOne(x => x.EndReader)
             .WithMany()
             .HasForeignKey(x => x.EndReaderId)
             .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.SetNull);

            // One open session per worker (SQLite filtered unique index)
            e.HasIndex(ws => ws.WorkerId)
             .HasFilter("\"EndUtc\" IS NULL")
             .IsUnique();
        });

        base.OnModelCreating(b);
    }
}
