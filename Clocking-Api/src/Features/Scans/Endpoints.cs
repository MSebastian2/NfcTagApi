using Clocking.Api.Data;
using Clocking.Api.Data.Dtos;
using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Features.Scans;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapScanEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/scans");
        g.MapPost("", HandleScan);
        // convenience alias:
        app.MapPost("/punch", HandleScan);
        return app;
    }

    // Assumptions:
    // - Worker has a unique TagUid (uppercase hex) and IsActive
    // - WorkSession has EndUtc nullable; open session is EndUtc == null
    // - Reader has Code (string, unique) and IsActive
    // - Scan logs raw events
    private static async Task<IResult> HandleScan(AppDbContext db, IConfiguration cfg, ScanRequestDto dto)
    {
        var uid = dto.TagUid?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(uid)) return Results.BadRequest(new { message = "tag uid required" });

        // Reader (optional create-on-first-use)
        Reader? reader = null;
        if (!string.IsNullOrWhiteSpace(dto.ReaderCode))
        {
            var code = dto.ReaderCode!.Trim();
            reader = await db.Readers.FirstOrDefaultAsync(r => r.Code == code);
            if (reader is null)
            {
                reader = new Reader { Code = code, Name = code, IsActive = true };
                db.Readers.Add(reader);
                await db.SaveChangesAsync();
            }
        }

        var worker = await db.Workers.FirstOrDefaultAsync(w => w.TagUid == uid && w.IsActive);
        if (worker is null) return Results.NotFound(new { message = "worker not found/active for tag", uid });

        var now = DateTimeOffset.UtcNow;
        var windowSec = cfg.GetValue<int>("Punching:DuplicateWindowSeconds", 5);

        // Dedupe: last scan for this worker+reader within window
        var recent = await db.Scans
            .Where(s => s.WorkerId == worker.Id && (reader == null || s.ReaderId == reader.Id))
            .OrderByDescending(s => s.OccurredAtUtc)
            .FirstOrDefaultAsync();

        if (recent != null && (now - recent.OccurredAtUtc).TotalSeconds < windowSec)
        {
            return Results.Ok(new ScanResultDto(created: false, ignored: true, action: "ignored", at: recent.OccurredAtUtc));
        }

        // Log the raw scan
        var scan = new Scan
        {
            WorkerId = worker.Id,
            ReaderId = reader?.Id,
            Uid = uid,
            OccurredAtUtc = now
        };
        db.Scans.Add(scan);

        // Toggle session (IN if no open session; OUT if open)
        var open = await db.WorkSessions
            .Where(ws => ws.WorkerId == worker.Id && ws.EndUtc == null)
            .OrderByDescending(ws => ws.StartUtc)
            .FirstOrDefaultAsync();

        string action;
        if (open is null)
        {
            // clock in
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
            // clock out
            open.EndUtc = now;
            open.EndReaderId = reader?.Id;
            action = "out";
        }

        await db.SaveChangesAsync();
        return Results.Created($"/scans/{scan.Id}", new ScanResultDto(true, false, action, now));
    }
}
