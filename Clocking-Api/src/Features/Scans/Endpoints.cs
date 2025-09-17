using Clocking.Api.Data;
using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Features.Scans;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapScanEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/scans");

        // Main punch endpoint
        g.MapPost("", HandleScan);

        // Alias for convenience
        app.MapPost("/punch", HandleScan);

        return g;
    }

    /// <summary>
    /// Handles a single NFC scan. Dedupe window prevents rapid double-taps.
    /// If the worker has an open session -> clock OUT; otherwise -> clock IN.
    /// </summary>
    private static async Task<IResult> HandleScan(AppDbContext db, IConfiguration cfg, PunchRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TagUid))
            return Results.BadRequest(new { message = "TagUid is required" });

        var uid = dto.TagUid.Trim().ToUpperInvariant();

        // Resolve optional reader (create-on-first-use so dev UX is easy)
        Reader? reader = null;
        if (!string.IsNullOrWhiteSpace(dto.ReaderCode))
        {
            var code = dto.ReaderCode.Trim().ToUpperInvariant();
            reader = await db.Readers.FirstOrDefaultAsync(r => r.Code == code);
            if (reader is null)
            {
                reader = new Reader { Code = code, Name = dto.ReaderCode.Trim(), IsActive = true };
                db.Readers.Add(reader);
                await db.SaveChangesAsync(); // need ID for FK
            }
        }

        // Find active worker by tag
        var worker = await db.Workers.FirstOrDefaultAsync(w => w.TagUid == uid && w.IsActive);
        if (worker is null)
            return Results.NotFound(new { message = "worker not found/active for tag", uid });

        var now = DateTimeOffset.UtcNow;
        var windowSec = cfg.GetValue<int>("Punching:DuplicateWindowSeconds", 5);

        // Dedupe: if the last scan for this worker (optionally same reader) is within the window, ignore
        var recent = await db.Scans
            .Where(s => s.WorkerId == worker.Id && (reader == null || s.ReaderId == reader.Id))
            .OrderByDescending(s => s.OccurredAtUtc)
            .FirstOrDefaultAsync();

        if (recent != null && (now - recent.OccurredAtUtc).TotalSeconds < windowSec)
        {
            return Results.Ok(new PunchResultDto(
                created: false,
                ignored: true,
                action: "ignored",
                at: recent.OccurredAtUtc
            ));
        }

        // Log raw scan
        var scan = new Scan
        {
            WorkerId = worker.Id,
            ReaderId = reader?.Id,
            Uid = uid,
            OccurredAtUtc = now,
            Origin = ScanOrigin.Nfc
        };
        db.Scans.Add(scan);

        // Toggle session
        var open = await db.WorkSessions
            .Where(ws => ws.WorkerId == worker.Id && ws.EndUtc == null)
            .OrderByDescending(ws => ws.StartUtc)
            .FirstOrDefaultAsync();

        string action;
        if (open is null)
        {
            // Clock IN
            db.WorkSessions.Add(new WorkSession
            {
                WorkerId = worker.Id,
                StartUtc = now,
                StartReaderId = reader?.Id
            });
            action = "in";
        }
        else
        {
            // Clock OUT
            open.EndUtc = now;
            open.EndReaderId = reader?.Id;
            action = "out";
        }

        await db.SaveChangesAsync();

        return Results.Created($"/scans/{scan.Id}", new PunchResultDto(
            created: true,
            ignored: false,
            action: action,
            at: now
        ));
    }

    // Request/response contracts local to this feature
    private record PunchRequestDto(string TagUid, string? ReaderCode);
    private record PunchResultDto(bool created, bool ignored, string action, DateTimeOffset at);
}
