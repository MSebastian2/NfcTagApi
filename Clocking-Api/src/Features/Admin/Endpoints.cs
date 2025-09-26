using Clocking.Api.Data;
using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Features.Admin;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminApi(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/admin");

        // WORKERS
        g.MapGet("/workers", async (AppDbContext db) =>
            await db.Workers.OrderBy(w => w.FullName).ToListAsync());

        g.MapPost("/workers", async (Worker w, AppDbContext db) =>
        {
            db.Workers.Add(w);
            await db.SaveChangesAsync();
            return Results.Created($"/admin/workers/{w.Id}", w);
        });

        g.MapPut("/workers/{id:int}", async (int id, Worker input, AppDbContext db) =>
        {
            var w = await db.Workers.FindAsync(id);
            if (w is null) return Results.NotFound();
            w.FullName = input.FullName;
            w.TagUid   = input.TagUid;        // null until assigned
            w.IsActive = input.IsActive;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        g.MapDelete("/workers/{id:int}", async (int id, AppDbContext db) =>
        {
            var w = await db.Workers.FindAsync(id);
            if (w is null) return Results.NotFound();
            db.Workers.Remove(w);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // READERS
        g.MapGet("/readers", async (AppDbContext db) =>
            await db.Readers.OrderBy(r => r.Code).ToListAsync());

        g.MapPost("/readers", async (Reader r, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(r.Code)) return Results.BadRequest(new { message = "Code required" });
            r.IsActive = true;
            r.ApiKey ??= Guid.NewGuid().ToString("N"); // add ApiKey property to Reader entity
            db.Readers.Add(r);
            await db.SaveChangesAsync();
            return Results.Created($"/admin/readers/{r.Id}", r);
        });

        g.MapPut("/readers/{id:int}", async (int id, Reader input, AppDbContext db) =>
        {
            var r = await db.Readers.FindAsync(id);
            if (r is null) return Results.NotFound();
            r.Name     = input.Name;
            r.Code     = input.Code;
            r.Type     = input.Type;
            r.IsActive = input.IsActive;
            r.LocationId = input.LocationId;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        g.MapDelete("/readers/{id:int}", async (int id, AppDbContext db) =>
        {
            var r = await db.Readers.FindAsync(id);
            if (r is null) return Results.NotFound();
            db.Readers.Remove(r);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // PAIRING: return config for Raspberry Pi
        g.MapGet("/readers/{id:int}/config", async (int id, AppDbContext db, HttpContext ctx) =>
        {
            var r = await db.Readers.FindAsync(id);
            if (r is null) return Results.NotFound();
            var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
            return Results.Ok(new {
                readerCode = r.Code,
                apiKey = r.ApiKey,     // add this column
                apiBase = baseUrl,
                punchEndpoint = $"{baseUrl}/punch"
            });
        });

        // Assign an NFC tag to a worker (given UID read by a gateway)
        g.MapPost("/workers/{id:int}/assign-tag", async (int id, AssignTagDto dto, AppDbContext db) =>
        {
            var w = await db.Workers.FindAsync(id);
            if (w is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(dto.Uid)) return Results.BadRequest(new { message = "UID required" });

            // Enforce uniqueness
            var taken = await db.Workers.AnyAsync(x => x.TagUid == dto.Uid && x.Id != id);
            if (taken) return Results.Conflict(new { message = "UID already assigned to another worker." });

            w.TagUid = dto.Uid;
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Tag assigned", worker = w.FullName, uid = w.TagUid });
        });

        // REPORTS: sessions between dates, optional worker
        g.MapGet("/reports/clocking", async (DateTimeOffset from, DateTimeOffset to, int? workerId, AppDbContext db) =>
        {
            var q = db.WorkSessions
                .Include(ws => ws.Worker)
                .Include(ws => ws.StartReader)
                .Include(ws => ws.EndReader)
                .Where(ws => ws.StartUtc >= from && ws.StartUtc <= to);

            if (workerId is not null)
                q = q.Where(ws => ws.WorkerId == workerId);

            var rows = await q
                .OrderBy(ws => ws.WorkerId).ThenBy(ws => ws.StartUtc)
                .Select(ws => new ReportRow(
                    ws.Worker!.FullName, // Worker is required in our model; null-forgive for compiler
                    ws.StartUtc,
                    ws.EndUtc,
                    ws.StartReader != null ? ws.StartReader.Code : "",
                    ws.EndReader   != null ? ws.EndReader.Code   : "",
                    ws.EndUtc.HasValue ? (ws.EndUtc.Value - ws.StartUtc).TotalMinutes : 0
                ))
                .ToListAsync();

            var totals = rows
                .GroupBy(r => r.Worker)
                .Select(g => new { worker = g.Key, minutes = g.Sum(x => x.Minutes) })
                .ToList();

            return Results.Ok(new { rows, totals });
        });

        return app;
    }

    public record AssignTagDto(string Uid);
    public record ReportRow(
    string Worker,
    DateTimeOffset StartUtc,
    DateTimeOffset? EndUtc,
    string StartReader,
    string EndReader,
    double Minutes);

}
