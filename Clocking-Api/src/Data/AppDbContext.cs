using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
        // SQLite hates ORDER BY DateTimeOffset. We won’t order in SQL anymore,
        // but map offsets explicitly so future queries don’t surprise you.
        var dto = new DateTimeOffsetToBinaryConverter();

        // Worker
        b.Entity<Worker>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.TagUid).HasMaxLength(64);
            e.HasIndex(x => x.TagUid).IsUnique();   // one tag per worker
            e.Property(x => x.IsActive).HasDefaultValue(true);

            e.HasMany(x => x.WorkSessions)
             .WithOne(ws => ws.Worker)
             .HasForeignKey(ws => ws.WorkerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Scans)
             .WithOne(s => s.Worker)
             .HasForeignKey(s => s.WorkerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Reader
        b.Entity<Reader>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.IsActive).HasDefaultValue(true);

            e.HasOne(x => x.Location)
             .WithMany(l => l.Readers)
             .HasForeignKey(x => x.LocationId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Location
        b.Entity<Location>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Code).HasMaxLength(64);
            e.HasIndex(x => x.Code);
        });

        // Scan (raw tap log)
        b.Entity<Scan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Uid).HasMaxLength(64);       // optional raw card UID
            e.Property(x => x.WhenUtc).IsRequired();       // used by endpoint
            e.Property(x => x.Type).IsRequired();          // ScanType enum

            e.HasOne(x => x.Worker)
             .WithMany(w => w.Scans)
             .HasForeignKey(x => x.WorkerId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Reader)
             .WithMany()
             .HasForeignKey(x => x.ReaderId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => x.WhenUtc);
        });

        // WorkSession (in/out pair)
        b.Entity<WorkSession>(e =>
        {
            e.HasKey(x => x.Id);

            // If these are DateTimeOffset in your entity (likely), map them to a sortable binary.
            e.Property(x => x.StartUtc).HasConversion(dto).IsRequired();
            e.Property(x => x.EndUtc).HasConversion(dto); // null when still clocked-in

            e.HasOne(x => x.Worker)
             .WithMany(w => w.WorkSessions)
             .HasForeignKey(x => x.WorkerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.StartReader)
             .WithMany()
             .HasForeignKey(x => x.StartReaderId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.EndReader)
             .WithMany()
             .HasForeignKey(x => x.EndReaderId)
             .OnDelete(DeleteBehavior.SetNull);

            // Enforce: at most one open session per worker
            e.HasIndex(ws => ws.WorkerId)
             .HasFilter("\"EndUtc\" IS NULL")  // SQLite syntax
             .IsUnique();
        });

        base.OnModelCreating(b);
    }
}
