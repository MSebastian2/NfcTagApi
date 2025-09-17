using Clocking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Data;

public static class Seed
{
    public static async Task EnsureCreatedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        if (!await db.Locations.AnyAsync())
        {
            var loc = new Location { Name = "HQ Office", Type = LocationType.office };
            db.Locations.Add(loc);

            var deviceKey = Guid.NewGuid().ToString("N"); // print once
            var reader = new Reader { Name = "Front Door", Location = loc, ApiKey = deviceKey };
            db.Readers.Add(reader);

            var worker = new Worker { FullName = "Ana Popescu", Department = "Office" };
            db.Workers.Add(worker);

            db.NfcCredentials.Add(new Nfccredential { Worker = worker, UidHex = "04AABBCCDD22" });

            await db.SaveChangesAsync();
            Console.WriteLine("=== Seed complete ===");
            Console.WriteLine($"Device API key: {deviceKey}");
            Console.WriteLine("Sample NFC UID:  04AABBCCDD22");
        }
    }
}
