using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Data;

public static class Seed
{
    /// <summary>
    /// Ensures baseline sample data exists for Development.
    /// Safe to call multiple times (idempotent).
    /// </summary>
    public static async Task EnsureSeedDataAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Apply migrations (in case caller didn’t)
        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync(ct);
        }

        // --- Locations ---
        var hq = await db.Locations.FirstOrDefaultAsync(l => l.Code == "HQ", ct);
        if (hq is null)
        {
            hq = new Location { Name = "Headquarters", Code = "HQ" };
            db.Locations.Add(hq);
            await db.SaveChangesAsync(ct);
        }

        // --- Readers ---
        var readerLab = await db.Readers.FirstOrDefaultAsync(r => r.Code == "ACR122U-LAB", ct);
        if (readerLab is null)
        {
            readerLab = new Reader { Code = "ACR122U-LAB", Name = "Lab Door", IsActive = true, LocationId = hq.Id };
            db.Readers.Add(readerLab);
        }

        var readerFront = await db.Readers.FirstOrDefaultAsync(r => r.Code == "ACR122U-FRONT", ct);
        if (readerFront is null)
        {
            readerFront = new Reader { Code = "ACR122U-FRONT", Name = "Front Desk", IsActive = true, LocationId = hq.Id };
            db.Readers.Add(readerFront);
        }

        // --- Workers ---
        var ada = await db.Workers.FirstOrDefaultAsync(w => w.TagUid == "04A224FF112233", ct);
        if (ada is null)
        {
            ada = new Worker
            {
                FullName = "Ada Lovelace",
                TagUid = "04A224FF112233",
                IsActive = true
            };
            db.Workers.Add(ada);
        }

        var alan = await db.Workers.FirstOrDefaultAsync(w => w.TagUid == "04112233445566", ct);
        if (alan is null)
        {
            alan = new Worker
            {
                FullName = "Alan Turing",
                TagUid = "04112233445566",
                IsActive = true
            };
            db.Workers.Add(alan);
        }

        await db.SaveChangesAsync(ct);

        // --- Optionally create an open WorkSession for Ada (if none exists) ---
        var now = DateTimeOffset.UtcNow;
        var adaOpen = await db.WorkSessions
            .AnyAsync(ws => ws.WorkerId == ada.Id && ws.EndUtc == null, ct);

        if (!adaOpen)
        {
            db.WorkSessions.Add(new WorkSession
            {
                WorkerId = ada.Id,
                StartUtc = now.AddHours(-1),
                StartReaderId = readerLab.Id
            });

            // also log a corresponding Scan
            db.Scans.Add(new Scan
            {
                WorkerId = ada.Id,
                ReaderId = readerLab.Id,
                Uid = ada.TagUid,
                OccurredAtUtc = now.AddHours(-1)
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
