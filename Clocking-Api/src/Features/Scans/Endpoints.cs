using Microsoft.EntityFrameworkCore;
using Clocking.Api.Data;
using Clocking.Api.Data.Entities;

namespace Clocking.Api.Features.Scans;

public static class ScanEndpoints
{
    public static IEndpointRouteBuilder MapScanEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/punch", HandlePunch);
        return app;
    }

    private static async Task<IResult> HandlePunch(PunchRequest req, AppDbContext db)
    {
        // Guard the request payload explicitly
        var tagUid = req.TagUid?.Trim();
        var readerCode = req.ReaderCode?.Trim();

        if (string.IsNullOrWhiteSpace(tagUid) || string.IsNullOrWhiteSpace(readerCode))
            return Results.BadRequest(new { error = "TagUid and ReaderCode are required." });

        // Lookups that can be null -> checked before use
        var worker = await db.Workers.SingleOrDefaultAsync(w => w.TagUid == tagUid);
        if (worker is null)
            return Results.BadRequest(new { error = $"Unknown tag '{tagUid}'." });

        var reader = await db.Readers.SingleOrDefaultAsync(r => r.Code == readerCode);
        if (reader is null)
            return Results.BadRequest(new { error = $"Unknown reader '{readerCode}'." });

        var nowUtc = DateTime.UtcNow;

        // Always record the raw scan
        db.Scans.Add(new Scan
        {
            WorkerId = worker.Id,
            ReaderId = reader.Id,
            WhenUtc = nowUtc,
            Type = ScanType.Unknown
        });

        // Try to close an open session; if none, start one
        // IMPORTANT: no ORDER BY on DateTimeOffset against SQLite
        var open = await db.WorkSessions
            .Where(s => s.WorkerId == worker.Id && s.EndUtc == null)
            .SingleOrDefaultAsync();

        if (open is null)
        {
            db.WorkSessions.Add(new WorkSession
            {
                WorkerId = worker.Id,
                StartUtc = nowUtc,
                StartReaderId = reader.Id
            });

            db.Scans.Add(new Scan
            {
                WorkerId = worker.Id,
                ReaderId = reader.Id,
                WhenUtc = nowUtc,
                Type = ScanType.In
            });

            await db.SaveChangesAsync();
            return Results.Ok(new { status = "opened", startedAtUtc = nowUtc });
        }
        else
        {
            // Assign to a local so the analyzer knows it's non-null when computing duration
            var endedAtUtc = nowUtc;
            open.EndUtc = endedAtUtc;
            open.EndReaderId = reader.Id;

            db.Scans.Add(new Scan
            {
                WorkerId = worker.Id,
                ReaderId = reader.Id,
                WhenUtc = endedAtUtc,
                Type = ScanType.Out
            });

            await db.SaveChangesAsync();
            var minutes = (endedAtUtc - open.StartUtc).TotalMinutes;

            return Results.Ok(new { status = "closed", endedAtUtc, minutes });
        }
    }
}

// Record with required properties; binder can still pass nulls, so we guard above.
public sealed class PunchRequest
{
    public required string TagUid { get; init; }
    public required string ReaderCode { get; init; }
}
