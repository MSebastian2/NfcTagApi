using System.Text.RegularExpressions;
using Clocking.Api.Data;
using Clocking.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Features.Workers;

public static class WorkersEndpoints
{
    public static IEndpointRouteBuilder MapWorkersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/workers");

        // POST /v1/workers
        group.MapPost("", async ([FromBody] CreateWorkerRequest body, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(body.full_name) || string.IsNullOrWhiteSpace(body.department))
                return Results.BadRequest(new { error = "full_name and department are required" });

            var w = new Worker {
                FullName = body.full_name.Trim(),
                Department = body.department.Trim(),
                ExternalId = string.IsNullOrWhiteSpace(body.external_id) ? null : body.external_id.Trim(),
                Role = string.IsNullOrWhiteSpace(body.role) ? null : body.role.Trim()
            };
            db.Workers.Add(w);
            await db.SaveChangesAsync();
            return Results.Created($"/v1/workers/{w.Id}", new { id = w.Id, full_name = w.FullName, department = w.Department });
        });

        // POST /v1/workers/{id}/nfc
        group.MapPost("{id:int}/nfc", async (int id, [FromBody] AssignNfcRequest body, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(body.uid_hex) || !Regex.IsMatch(body.uid_hex, "^[0-9A-Fa-f]+$"))
                return Results.BadRequest(new { error = "uid_hex must be hex string" });

            var worker = await db.Workers.FindAsync(id);
            if (worker is null) return Results.NotFound(new { error = "worker_not_found" });

            var uid = body.uid_hex.ToUpperInvariant();

            // Optional rule: deactivate other active cards for this worker
            if (body.deactivate_others)
                await db.NfcCredentials.Where(c => c.WorkerId == id && c.IsActive).ExecuteUpdateAsync(
                    s => s.SetProperty(c => c.IsActive, false));

            // Ensure UID is unique
            var exists = await db.NfcCredentials.AnyAsync(c => c.UidHex == uid && c.IsActive);
            if (exists) return Results.Conflict(new { error = "uid_already_assigned" });

            db.NfcCredentials.Add(new Nfccredential { WorkerId = id, UidHex = uid, IsActive = true });
            await db.SaveChangesAsync();
            return Results.Ok(new { worker_id = id, uid_hex = uid, active = true });
        });

        // GET /v1/workers/{id}/sessions?from=2025-09-01&to=2025-09-30
        group.MapGet("{id:int}/sessions", async (int id, DateTime? from, DateTime? to, AppDbContext db) =>
        {
            var q = db.WorkSessions.AsNoTracking().Where(s => s.WorkerId == id && s.Status == SessionStatus.closed);
            if (from.HasValue) q = q.Where(s => s.CheckInAt >= from.Value);
            if (to.HasValue)   q = q.Where(s => s.CheckInAt <  to.Value);

            var list = await q
                .OrderByDescending(s => s.CheckInAt)
                .Select(s => new {
                    s.Id,
                    s.LocationId,
                    check_in_at_utc = s.CheckInAt.UtcDateTime.ToString("o"),
                    check_out_at_utc = s.CheckOutAt!.Value.UtcDateTime.ToString("o"),
                    s.DurationSec
                }).ToListAsync();

            return Results.Ok(list);
        });

        return app;
    }
}

public record CreateWorkerRequest(string full_name, string department, string? role, string? external_id);
public record AssignNfcRequest(string uid_hex, bool deactivate_others = true);
