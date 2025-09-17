using Clocking.Api.Data;
using Clocking.Api.Data.Dtos;
using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Features.Readers;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapReaderEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/readers");

        // List readers
        g.MapGet("", async (AppDbContext db) =>
        {
            var rows = await db.Readers
                .Include(r => r.Location)
                .OrderBy(r => r.Code)
                .Select(r => new
                {
                    r.Id,
                    r.Code,
                    r.Name,
                    r.IsActive,
                    r.Type,
                    Location = r.Location == null ? null : new { r.Location.Id, r.Location.Code, r.Location.Name }
                })
                .ToListAsync();

            return Results.Ok(rows);
        });

        // Get reader by code
        g.MapGet("/{code}", async (AppDbContext db, string code) =>
        {
            var key = code.Trim().ToUpperInvariant();
            var r = await db.Readers
                .Include(x => x.Location)
                .FirstOrDefaultAsync(x => x.Code == key);

            if (r is null) return Results.NotFound(new { message = "reader not found", code = key });

            return Results.Ok(new
            {
                r.Id,
                r.Code,
                r.Name,
                r.IsActive,
                r.Type,
                Location = r.Location == null ? null : new { r.Location.Id, r.Location.Code, r.Location.Name }
            });
        });

        // Create reader
        g.MapPost("", async (AppDbContext db, CreateReaderDto dto) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return Results.BadRequest(new { message = "Code is required" });

            var code = dto.Code.Trim().ToUpperInvariant();

            if (await db.Readers.AnyAsync(r => r.Code == code))
                return Results.Conflict(new { message = "Reader code already exists", code });

            var reader = new Reader
            {
                Code = code,
                Name = string.IsNullOrWhiteSpace(dto.Name) ? code : dto.Name!.Trim(),
                IsActive = dto.IsActive,
                Type = ReaderType.Fixed
            };

            // Attach to a location if provided
            if (dto.LocationId.HasValue || !string.IsNullOrWhiteSpace(dto.LocationCode))
            {
                var loc = await ResolveLocationAsync(db, dto.LocationId, dto.LocationCode);
                if (loc is null && !string.IsNullOrWhiteSpace(dto.LocationCode))
                {
                    // create by code if it doesn't exist
                    loc = new Location { Code = dto.LocationCode!.Trim().ToUpperInvariant(), Name = dto.LocationCode!.Trim(), IsActive = true };
                    db.Locations.Add(loc);
                    await db.SaveChangesAsync();
                }

                reader.LocationId = loc?.Id;
            }

            db.Readers.Add(reader);
            await db.SaveChangesAsync();

            return Results.Created($"/readers/{reader.Code}", new
            {
                reader.Id,
                reader.Code,
                reader.Name,
                reader.IsActive,
                reader.Type,
                reader.LocationId
            });
        });

        // Assign reader to a location
        g.MapPost("/assign-location", async (AppDbContext db, AssignReaderLocationDto dto) =>
        {
            if (string.IsNullOrWhiteSpace(dto.ReaderCode))
                return Results.BadRequest(new { message = "ReaderCode is required" });

            var code = dto.ReaderCode.Trim().ToUpperInvariant();
            var reader = await db.Readers.FirstOrDefaultAsync(r => r.Code == code);
            if (reader is null) return Results.NotFound(new { message = "reader not found", code });

            var location = await ResolveLocationAsync(db, dto.LocationId, dto.LocationCode);
            if (location is null)
            {
                if (!string.IsNullOrWhiteSpace(dto.LocationCode))
                {
                    // Create location if referenced by code but missing
                    location = new Location
                    {
                        Code = dto.LocationCode!.Trim().ToUpperInvariant(),
                        Name = dto.LocationCode!.Trim(),
                        IsActive = true
                    };
                    db.Locations.Add(location);
                    await db.SaveChangesAsync();
                }
                else
                {
                    // Explicitly unassign if no location provided
                    reader.LocationId = null;
                    await db.SaveChangesAsync();
                    return Results.Ok(new { reader.Code, reader.LocationId });
                }
            }

            reader.LocationId = location.Id;
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                reader.Code,
                Location = new { location.Id, location.Code, location.Name }
            });
        });

        // Activate/deactivate a reader
        g.MapPatch("/{code}/status", async (AppDbContext db, string code, StatusDto body) =>
        {
            var key = code.Trim().ToUpperInvariant();
            var reader = await db.Readers.FirstOrDefaultAsync(r => r.Code == key);
            if (reader is null) return Results.NotFound(new { message = "reader not found", code = key });

            reader.IsActive = body.IsActive;
            await db.SaveChangesAsync();
            return Results.Ok(new { reader.Code, reader.IsActive });
        });

        return g;
    }

    private static async Task<Location?> ResolveLocationAsync(AppDbContext db, int? id, string? code)
    {
        if (id.HasValue)
            return await db.Locations.FirstOrDefaultAsync(l => l.Id == id.Value);

        if (!string.IsNullOrWhiteSpace(code))
        {
            var key = code.Trim().ToUpperInvariant();
            return await db.Locations.FirstOrDefaultAsync(l => l.Code == key);
        }

        return null;
    }

    private record StatusDto(bool IsActive);
}
