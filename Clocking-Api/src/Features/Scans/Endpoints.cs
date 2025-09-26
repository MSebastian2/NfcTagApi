using Clocking.Api.Data;
using Clocking.Api.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Features.Scans;

public static class ScanEndpoints
{
    public record PunchRequest(string TagUid, string ReaderCode);

    public static IEndpointRouteBuilder MapScanEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/punch", HandlePunch);
        return app;
    }

    private static async Task<IResult> HandlePunch(PunchRequest req, AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(req.TagUid) || string.IsNullOrWhiteSpace(req.ReaderCode))
            return Results.BadRequest(new { error = "TagUid and ReaderCode are required." });

        var reader = await db.Readers.SingleOrDefaultAsync(r => r.Code == req.ReaderCode);
        if (reader is null) return Results.NotFound(new { error = $"Reader '{req.ReaderCode}' not found" });

        var worker = await db.Workers.SingleOrDefaultAsync(w => w.TagUid == req.TagUid && w.IsActive);
        if (worker is null) return Results.NotFound(new { error = $"Active worker with TagUid '{req.TagUid}' not found" });

        var open = await db.WorkSessions.SingleOrDefaultAsync(ws => ws.WorkerId == worker.Id && ws.EndUtc == null);

        var now = DateTime.UtcNow;

        if (open is null)
        {
            // create IN scan  ⬇⬇⬇
            db.Scans.Add(new Scan
            {
                WorkerId = worker.Id,
                ReaderId = reader.Id,
                WhenUtc  = now,
                Type     = ScanType.In,         // enum
                Origin   = ScanOrigin.Api,      // enum
                Uid      = req.TagUid
            });

            var session = new WorkSession
            {
                WorkerId      = worker.Id,
                StartReaderId = reader.Id,
                StartUtc      = now
            };

            db.WorkSessions.Add(session);
            await db.SaveChangesAsync();

            return Results.Created($"/work-sessions/{session.Id}", new
            {
                message = "Clock-in recorded",
                session.Id,
                worker = worker.FullName,
                reader = reader.Code,
                at = now
            });
        }
        else
        {
            // create OUT scan  ⬇⬇⬇
            db.Scans.Add(new Scan
            {
                WorkerId = worker.Id,
                ReaderId = reader.Id,
                WhenUtc  = now,
                Type     = ScanType.Out,        // enum
                Origin   = ScanOrigin.Api,      // enum
                Uid      = req.TagUid
            });

            open.EndReaderId = reader.Id;
            open.EndUtc      = now;

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                message = "Clock-out recorded",
                open.Id,
                worker = worker.FullName,
                reader = reader.Code,
                started = open.StartUtc,
                ended   = open.EndUtc
            });
        }
    }
}
