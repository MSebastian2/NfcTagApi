using Clocking.Api.Data;
using Clocking.Api.Data.Dtos;
using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Features.Workers;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapWorkerEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/workers");

        // List workers
        g.MapGet("", async (AppDbContext db) =>
        {
            var rows = await db.Workers
                .OrderBy(w => w.FullName)
                .Select(w => new
                {
                    w.Id,
                    w.FullName,
                    w.TagUid,
                    w.IsActive,
                    OpenSession = db.WorkSessions.Any(ws => ws.WorkerId == w.Id && ws.EndUtc == null)
                })
                .ToListAsync();

            return Results.Ok(rows);
        });

        // Get worker by id
        g.MapGet("/{id:int}", async (AppDbContext db, int id) =>
        {
            var w = await db.Workers.FirstOrDefaultAsync(x => x.Id == id);
            if (w is null) return Results.NotFound();

            var open = await db.WorkSessions.AnyAsync(ws => ws.WorkerId == w.Id && ws.EndUtc == null);
            return Results.Ok(new
            {
                w.Id,
                w.FullName,
                w.TagUid,
                w.IsActive,
                OpenSession = open
            });
        });

        // Create worker
        g.MapPost("", async (AppDbContext db, CreateWorkerDto dto) =>
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                return Results.BadRequest(new { message = "FullName is required" });

            string? tag = null;
            if (!string.IsNullOrWhiteSpace(dto.TagUid))
            {
                tag = dto.TagUid!.Trim().ToUpperInvariant();
                var exists = await db.Workers.AnyAsync(w => w.TagUid == tag);
                if (exists)
                    return Results.Conflict(new { message = "TagUid already assigned to another worker", tagUid = tag });
            }

            var w = new Worker
            {
                FullName = dto.FullName.Trim(),
                TagUid = tag,
                IsActive = dto.IsActive
            };

            db.Workers.Add(w);
            await db.SaveChangesAsync();

            return Results.Created($"/workers/{w.Id}", new
            {
                w.Id,
                w.FullName,
                w.TagUid,
                w.IsActive
            });
        });

        // Update (partial)
        g.MapPatch("/{id:int}", async (AppDbContext db, int id, UpdateWorkerDto dto) =>
        {
            var w = await db.Workers.FirstOrDefaultAsync(x => x.Id == id);
            if (w is null) return Results.NotFound();

            if (dto.FullName is not null)
            {
                if (string.IsNullOrWhiteSpace(dto.FullName))
                    return Results.BadRequest(new { message = "FullName cannot be empty when provided" });
                w.FullName = dto.FullName.Trim();
            }

            if (dto.TagUid is not null)
            {
                // empty string => clear tag
                if (dto.TagUid.Length == 0)
                {
                    w.TagUid = null;
                }
                else
                {
                    var tag = dto.TagUid.Trim().ToUpperInvariant();
                    var exists = await db.Workers.AnyAsync(x => x.Id != id && x.TagUid == tag);
                    if (exists)
                        return Results.Conflict(new { message = "TagUid already assigned to another worker", tagUid = tag });
                    w.TagUid = tag;
                }
            }

            if (dto.IsActive is bool active)
            {
                w.IsActive = active;
            }

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                w.Id,
                w.FullName,
                w.TagUid,
                w.IsActive
            });
        });

        // Activate/deactivate (shortcut)
        g.MapPatch("/{id:int}/status", async (AppDbContext db, int id, StatusDto body) =>
        {
            var w = await db.Workers.FirstOrDefaultAsync(x => x.Id == id);
            if (w is null) return Results.NotFound();

            w.IsActive = body.IsActive;
            await db.SaveChangesAsync();
            return Results.Ok(new { w.Id, w.IsActive });
        });

        // Optional: delete (soft-delete recommended; here just hard delete if no sessions)
        g.MapDelete("/{id:int}", async (AppDbContext db, int id) =>
        {
            var w = await db.Workers.FirstOrDefaultAsync(x => x.Id == id);
            if (w is null) return Results.NotFound();

            var hasSessions = await db.WorkSessions.AnyAsync(ws => ws.WorkerId == id);
            if (hasSessions)
                return Results.BadRequest(new { message = "Cannot delete worker with existing sessions. Consider deactivating instead." });

            db.Workers.Remove(w);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return g;
    }

    private record StatusDto(bool IsActive);
}
