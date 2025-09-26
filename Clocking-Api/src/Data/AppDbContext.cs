using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
// shorter name so the compiler stops whining even if your usings are cursed
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
        // Workers
        b.Entity<Worker>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.TagUid).HasMaxLength(64);
            e.HasIndex(x => x.TagUid).IsUnique();
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

        // Readers
        b.Entity<Reader>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200);

            e.HasOne(x => x.Location)
             .WithMany(l => l.Readers)
             .HasForeignKey(x => x.LocationId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Locations (you barely use it yet, but keep it coherent)
        b.Entity<Location>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Code).HasMaxLength(64);
            e.HasIndex(x => x.Code);
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // Scans
        b.Entity<Scan>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.WhenUtc).IsRequired();

            // map enums explicitly; NO HasDefaultValue(1) nonsense
            e.Property(x => x.Origin)
            .HasConversion<int>()
            .IsRequired();

            e.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

            e.Property(x => x.Uid).HasMaxLength(64);

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

        // WorkSessions (paired in/out)
        b.Entity<WorkSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StartUtc).IsRequired();
            e.Property(x => x.EndUtc); // null when open

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
             .IsUnique()
             .HasFilter("EndUtc IS NULL"); // SQLite filter syntax
        });

        base.OnModelCreating(b);
    }
}
