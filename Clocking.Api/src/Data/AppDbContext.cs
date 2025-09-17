using Clocking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Reader> Readers => Set<Reader>();
    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<Nfccredential> NfcCredentials => Set<Nfccredential>();
    public DbSet<Scan> Scans => Set<Scan>();
    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Reader>().HasIndex(r => r.ApiKey).IsUnique();
        b.Entity<Nfccredential>().HasIndex(c => c.UidHex).IsUnique();
        b.Entity<Scan>().HasIndex(s => s.IdempotencyKey).IsUnique();
        b.Entity<Scan>().HasIndex(s => new { s.UidHex, s.ScannedAt });

        b.Entity<WorkSession>().HasIndex(ws => new { ws.WorkerId, ws.Status });

        b.Entity<WorkSession>()
            .HasOne(ws => ws.ReaderIn)
            .WithMany(r => r.SessionsIn)
            .HasForeignKey(ws => ws.ReaderInId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<WorkSession>()
            .HasOne(ws => ws.ReaderOut)
            .WithMany(r => r.SessionsOut)
            .HasForeignKey(ws => ws.ReaderOutId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
