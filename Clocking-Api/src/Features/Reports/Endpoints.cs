using Clocking.Api.Data;
using Clocking.Api.Data.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Clocking.Api.Features.Reports;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/reports");

        // POST /reports/attendance  (body: ReportRangeDto)
        g.MapPost("/attendance", Attendance);

        // POST /reports/sessions    (body: ReportRangeDto)
        g.MapPost("/sessions", Sessions);

        // POST /reports/scans       (body: ReportRangeDto)
        g.MapPost("/scans", Scans);

        return g;
    }

    // ----------------------
    // Attendance (grouped)
    // ----------------------
    private static async Task<IResult> Attendance(AppDbContext db, ReportRangeDto dto)
    {
        var now = DateTimeOffset.UtcNow;
        var to = dto.ToUtc ?? now;
        var from = dto.FromUtc ?? to.AddDays(-30);
        if (from > to) (from, to) = (to, from);

        // Load sessions overlapping the range
        var sessions = await db.WorkSessions
            .Include(ws => ws.Worker)
            .Include(ws => ws.StartReader)!.ThenInclude(r => r!.Location)
            .Include(ws => ws.EndReader)!.ThenInclude(r => r!.Location)
            .Where(ws =>
                ws.StartUtc <= to &&
                (ws.EndUtc ?? now) >= from &&
                (dto.WorkerId == null || ws.WorkerId == dto.WorkerId))
            .ToListAsync();

        // Optional location / reader filters applied in-memory
        if (dto.LocationId is int locId)
        {
            sessions = sessions.Where(ws =>
                (ws.StartReader?.LocationId == locId) ||
                (ws.EndReader?.LocationId == locId))
            .ToList();
        }
        if (!string.IsNullOrWhiteSpace(dto.ReaderCode))
        {
            var code = dto.ReaderCode.Trim().ToUpperInvariant();
            sessions = sessions.Where(ws =>
                (ws.StartReader?.Code == code) ||
                (ws.EndReader?.Code == code))
            .ToList();
        }

        var groupBy = (dto.GroupBy ?? "day").Trim().ToLowerInvariant();

        var rows = sessions.Select(ws =>
        {
            var effStart = ws.StartUtc < from ? from : ws.StartUtc;
            var effEnd = (ws.EndUtc ?? now) > to ? to : (ws.EndUtc ?? now);
            if (effEnd < effStart) effEnd = effStart;

            var seconds = (effEnd - effStart).TotalSeconds;
            var key = groupBy switch
            {
                "week"  => $"{IsoWeekYear(effStart):D4}-W{IsoWeekOfYear(effStart):D2}",
                "month" => $"{effStart.Year:D4}-{effStart.Month:D2}",
                _       => effStart.UtcDateTime.ToString("yyyy-MM-dd")
            };

            return new
            {
                ws.WorkerId,
                WorkerName = ws.Worker?.FullName ?? $"Worker #{ws.WorkerId}",
                Period = key,
                Seconds = seconds
            };
        })
        // A single session may span days/weeks/months; for simplicity we bin by start time.
        // (If you need precise split across periods, we can segment per-day.)
        .GroupBy(x => new { x.WorkerId, x.WorkerName, x.Period })
        .Select(g => new {
            g.Key.WorkerId,
            g.Key.WorkerName,
            g.Key.Period,
            TotalSeconds = Math.Round(g.Sum(x => x.Seconds), 0),
            TotalHours = Math.Round(g.Sum(x => x.Seconds) / 3600.0, 2)
        })
        .OrderBy(x => x.WorkerName).ThenBy(x => x.Period)
        .ToList();

        return Results.Ok(new {
            FromUtc = from,
            ToUtc = to,
            GroupBy = groupBy,
            Rows = rows
        });
    }

    // ----------------------
    // Sessions (detailed)
    // ----------------------
    private static async Task<IResult> Sessions(AppDbContext db, ReportRangeDto dto)
    {
        var now = DateTimeOffset.UtcNow;
        var to = dto.ToUtc ?? now;
        var from = dto.FromUtc ?? to.AddDays(-30);
        if (from > to) (from, to) = (to, from);

        var q = db.WorkSessions
            .Include(ws => ws.Worker)
            .Include(ws => ws.StartReader)!.ThenInclude(r => r!.Location)
            .Include(ws => ws.EndReader)!.ThenInclude(r => r!.Location)
            .Where(ws =>
                ws.StartUtc <= to &&
                (ws.EndUtc ?? now) >= from &&
                (dto.WorkerId == null || ws.WorkerId == dto.WorkerId));

        var list = await q.ToListAsync();

        if (dto.LocationId is int locId)
        {
            list = list.Where(ws =>
                (ws.StartReader?.LocationId == locId) ||
                (ws.EndReader?.LocationId == locId))
            .ToList();
        }
        if (!string.IsNullOrWhiteSpace(dto.ReaderCode))
        {
            var code = dto.ReaderCode.Trim().ToUpperInvariant();
            list = list.Where(ws =>
                (ws.StartReader?.Code == code) ||
                (ws.EndReader?.Code == code))
            .ToList();
        }

        var rows = list.Select(ws =>
        {
            var effStart = ws.StartUtc < from ? from : ws.StartUtc;
            var effEnd = (ws.EndUtc ?? now) > to ? to : (ws.EndUtc ?? now);
            if (effEnd < effStart) effEnd = effStart;

            var totalSeconds = (effEnd - effStart).TotalSeconds;

            return new
            {
                ws.Id,
                ws.WorkerId,
                WorkerName = ws.Worker?.FullName,
                StartUtc = ws.StartUtc,
                EndUtc = ws.EndUtc,
                EffectiveStartUtc = effStart,
                EffectiveEndUtc = effEnd,
                TotalSeconds = Math.Round(totalSeconds, 0),
                TotalHours = Math.Round(totalSeconds / 3600.0, 2),
                StartReader = ws.StartReader == null ? null : new
                {
                    ws.StartReader.Id,
                    ws.StartReader.Code,
                    ws.StartReader.Name,
                    ws.StartReader.LocationId,
                    Location = ws.StartReader.Location == null ? null : new { ws.StartReader.Location.Id, ws.StartReader.Location.Code, ws.StartReader.Location.Name }
                },
                EndReader = ws.EndReader == null ? null : new
                {
                    ws.EndReader.Id,
                    ws.EndReader.Code,
                    ws.EndReader.Name,
                    ws.EndReader.LocationId,
                    Location = ws.EndReader.Location == null ? null : new { ws.EndReader.Location.Id, ws.EndReader.Location.Code, ws.EndReader.Location.Name }
                }
            };
        })
        .OrderBy(r => r.WorkerName).ThenBy(r => r.EffectiveStartUtc)
        .ToList();

        return Results.Ok(new {
            FromUtc = from,
            ToUtc = to,
            Count = rows.Count,
            Rows = rows
        });
    }

    // ----------------------
    // Scans (raw)
    // ----------------------
    private static async Task<IResult> Scans(AppDbContext db, ReportRangeDto dto)
    {
        var now = DateTimeOffset.UtcNow;
        var to = dto.ToUtc ?? now;
        var from = dto.FromUtc ?? to.AddDays(-30);
        if (from > to) (from, to) = (to, from);

        var q = db.Scans
            .Include(s => s.Worker)
            .Include(s => s.Reader)!.ThenInclude(r => r!.Location)
            .Where(s =>
                s.OccurredAtUtc >= from &&
                s.OccurredAtUtc <= to &&
                (dto.WorkerId == null || s.WorkerId == dto.WorkerId));

        if (!string.IsNullOrWhiteSpace(dto.ReaderCode))
        {
            var code = dto.ReaderCode.Trim().ToUpperInvariant();
            q = q.Where(s => s.Reader != null && s.Reader.Code == code);
        }
        if (dto.LocationId is int locId)
        {
            q = q.Where(s => s.Reader != null && s.Reader.LocationId == locId);
        }

        var rows = await q
            .OrderBy(s => s.OccurredAtUtc)
            .Select(s => new
            {
                s.Id,
                s.OccurredAtUtc,
                s.Uid,
                s.WorkerId,
                WorkerName = s.Worker!.FullName,
                Reader = s.Reader == null ? null : new
                {
                    s.Reader.Id,
                    s.Reader.Code,
                    s.Reader.Name,
                    s.Reader.LocationId,
                    Location = s.Reader.Location == null ? null : new { s.Reader.Location.Id, s.Reader.Location.Code, s.Reader.Location.Name }
                },
                s.Origin
            })
            .ToListAsync();

        return Results.Ok(new {
            FromUtc = from,
            ToUtc = to,
            Count = rows.Count,
            Rows = rows
        });
    }

    // ----------------------
    // Helpers
    // ----------------------
    private static int IsoWeekOfYear(DateTimeOffset date)
    {
        var d = date.UtcDateTime;
        var day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(d);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            d = d.AddDays(3);
        return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            d, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    private static int IsoWeekYear(DateTimeOffset date)
    {
        var d = date.UtcDateTime;
        var week = IsoWeekOfYear(date);
        if (d.Month == 1 && week >= 52) return d.Year - 1;
        if (d.Month == 12 && week == 1) return d.Year + 1;
        return d.Year;
    }
}
