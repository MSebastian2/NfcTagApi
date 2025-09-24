using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Clocking.Api.Data;
using Clocking.Api.Data.Entities;

public sealed class TestingWebAppFactory : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Clocking.Api.PublicApiMarker>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(), $"clocking-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // remove existing DbContextOptions<AppDbContext>
            var descriptors = services
                .Where(d => d.ServiceType.IsGenericType &&
                            d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={_dbPath}"));

            // build and migrate
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.Migrate();

            // seed minimal data for /punch
            if (!db.Readers.Any(r => r.Code == "LAB-001"))
                db.Readers.Add(new Reader { Code = "LAB-001", Name = "Lab door" });
            if (!db.Workers.Any(w => w.TagUid == "04AABBCCDD22"))
                db.Workers.Add(new Worker { FullName = "Demo Worker", TagUid = "04AABBCCDD22" });
            db.SaveChanges();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* best effort */ }
    }
}
