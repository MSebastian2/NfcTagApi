using Microsoft.EntityFrameworkCore;
using Clocking.Api.Data;
using Clocking.Api.Data.Entities;

namespace Clocking.Api.Features.Punch;

public static class PunchEndpoints
{
    public static IEndpointRouteBuilder MapPunchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/punch", async (PunchRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.TagUid) || string.IsNullOrWhiteSpace(req.ReaderCode))
                return Results.BadRequest(new { error = "TagUid and ReaderCode are required." });

            var worker = await db.Workers.SingleOrDefaultAsync(w => w.TagUid == req.TagUid);
            if (worker is null) return Results.BadRequest(new { error = $"Unknown tag '{req.TagUid}'." });

            var reader = await db.Readers.SingleOrDefaultAsync(r => r.Code == req.ReaderCode);
            if (reader is null) return Results.BadRequest(new { error = $"Unknown reader '{req.ReaderCode}'." });

            var now = DateTime.UtcNow;

            var open = await db.WorkSessions
                .Where(s => s.WorkerId == worker.Id && s.EndUtc == null)
                .OrderByDescending(s => s.StartUtc)
                .FirstOrDefaultAsync();

            if (open is null)
            {
                db.WorkSessions.Add(new WorkSession { WorkerId = worker.Id, StartUtc = now, StartReaderId = reader.Id });
                db.Scans.Add(new Scan { WorkerId = worker.Id, ReaderId = reader.Id, WhenUtc = now, Type = ScanType.In });
                await db.SaveChangesAsync();
                return Results.Ok(new { status = "opened", startedAtUtc = now });
            }
            else
            {
                open.EndUtc = now;
                open.EndReaderId = reader.Id;
                db.Scans.Add(new Scan { WorkerId = worker.Id, ReaderId = reader.Id, WhenUtc = now, Type = ScanType.Out });
                await db.SaveChangesAsync();
                return Results.Ok(new { status = "closed", endedAtUtc = now, minutes = (open.EndUtc.Value - open.StartUtc).TotalMinutes });
            }
        });

        return app;
    }
}

public record PunchRequest(string TagUid, string ReaderCode);
