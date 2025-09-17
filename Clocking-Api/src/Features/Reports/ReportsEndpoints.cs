using Clocking.Api.Data;
using Clocking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Features.Reports;

public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/reports");

        // GET /v1/reports/hours?from=2025-01-01&to=2025-01-31&location_type=construction
        group.MapGet("hours", async (DateTime? from, DateTime? to, LocationType? location_type, AppDbContext db) =>
        {
            var q = db.WorkSessions
                .AsNoTracking()
                .Where(ws => ws.Status == SessionStatus.closed)
                .Include(ws => ws.Worker)
                .Include(ws => ws.Location);

            if (from.HasValue) q = q.Where(ws => ws.CheckInAt >= from.Value);
            if (to.HasValue)   q = q.Where(ws => ws.CheckInAt <  to.Value);
            if (location_type.HasValue) q = q.Where(ws => ws.Location!.Type == location_type);

            var rows = await q
                .GroupBy(ws => new { ws.WorkerId, ws.Worker!.FullName, ws.Worker!.Department, LocType = ws.Location!.Type })
                .Select(g => new {
                    g.Key.WorkerId,
                    full_name = g.Key.FullName,
                    department = g.Key.Department,
                    location_type = g.Key.LocType.ToString(),
                    seconds_worked = g.Sum(x => x.DurationSec ?? 0)
                })
                .OrderBy(r => r.full_name)
                .ToListAsync();

            return Results.Ok(rows);
        });

        return app;
    }
}
