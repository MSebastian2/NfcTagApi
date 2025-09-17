using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<NfcTag> NfcTags => Set<NfcTag>();
    public DbSet<PunchEvent> PunchEvents => Set<PunchEvent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<NfcTag>().HasIndex(t => t.Uid).IsUnique();
        b.Entity<NfcTag>()
            .HasOne(t => t.Employee)
            .WithMany(e => e.Tags)
            .HasForeignKey(t => t.EmployeeId);

        b.Entity<PunchEvent>()
            .HasOne(p => p.Employee)
            .WithMany(e => e.Punches)
            .HasForeignKey(p => p.EmployeeId);
        b.Entity<PunchEvent>()
            .HasOne(p => p.Tag)
            .WithMany()
            .HasForeignKey(p => p.TagId);
    }
}
