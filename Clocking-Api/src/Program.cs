using Data;
using Data.Dtos;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core (SQLite)
builder.Services.AddDbContext<AppDbContext>(opts =>
{
    var cs = builder.Configuration.GetConnectionString("Default")!;
    opts.UseSqlite(cs);
});

// CORS (open for now—tighten later)
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

// Auto-migrate on boot (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/ping", () => Results.Ok(new { ok = true, at = DateTimeOffset.UtcNow }));

// Employees
app.MapPost("/employees", async (AppDbContext db, CreateEmployeeDto dto) =>
{
    var e = new Employee { FullName = dto.FullName.Trim(), IsActive = true };
    db.Employees.Add(e);
    await db.SaveChangesAsync();
    return Results.Created($"/employees/{e.Id}", new { e.Id, e.FullName, e.IsActive });
});

app.MapGet("/employees/{id:int}", async (AppDbContext db, int id) =>
{
    var e = await db.Employees.FindAsync(id);
    return e is null ? Results.NotFound() : Results.Ok(e);
});

// Tags
app.MapPost("/tags", async (AppDbContext db, CreateTagDto dto) =>
{
    var uid = dto.Uid.Trim().ToUpperInvariant();
    if (await db.NfcTags.AnyAsync(t => t.Uid == uid))
        return Results.Conflict(new { message = "Tag already exists" });

    var tag = new NfcTag { Uid = uid, Nickname = dto.Nickname?.Trim(), IsActive = true };
    db.NfcTags.Add(tag);
    await db.SaveChangesAsync();
    return Results.Created($"/tags/{tag.Id}", tag);
});

app.MapPost("/tags/assign", async (AppDbContext db, AssignTagDto dto) =>
{
    var uid = dto.Uid.Trim().ToUpperInvariant();
    var tag = await db.NfcTags.FirstOrDefaultAsync(t => t.Uid == uid);
    if (tag is null || !tag.IsActive) return Results.NotFound(new { message = "Tag not found/active" });

    var employee = await db.Employees.FindAsync(dto.EmployeeId);
    if (employee is null || !employee.IsActive) return Results.NotFound(new { message = "Employee not found/active" });

    tag.EmployeeId = employee.Id;
    await db.SaveChangesAsync();
    return Results.Ok(tag);
});

// Punch (tap)
app.MapPost("/punch", async (AppDbContext db, IConfiguration cfg, PunchDto dto) =>
{
    var uid = dto.TagUid.Trim().ToUpperInvariant();
    var tag = await db.NfcTags.Include(t => t.Employee).FirstOrDefaultAsync(t => t.Uid == uid);
    if (tag is null || !tag.IsActive) return Results.NotFound(new { message = "Tag not found/active" });
    if (tag.EmployeeId is null) return Results.BadRequest(new { message = "Tag not assigned to an employee" });

    var windowSec = cfg.GetValue<int>("Punching:DuplicateWindowSeconds", 5);
    var now = DateTimeOffset.UtcNow;

    // dedupe: ignore if same reader and within window
    var recent = await db.PunchEvents
        .Where(p => p.TagId == tag.Id && p.ReaderId == dto.ReaderId)
        .OrderByDescending(p => p.OccurredAtUtc)
        .FirstOrDefaultAsync();

    if (recent != null && (now - recent.OccurredAtUtc).TotalSeconds < windowSec)
        return Results.Ok(new { ignored = true, reason = "duplicate-window", recent.OccurredAtUtc });

    // Toggle: if last action was "in", do "out"; else "in"
    var last = await db.PunchEvents
        .Where(p => p.EmployeeId == tag.EmployeeId)
        .OrderByDescending(p => p.OccurredAtUtc)
        .FirstOrDefaultAsync();

    var nextAction = last?.Action == "in" ? "out" : "in";

    var evt = new PunchEvent
    {
        EmployeeId = tag.EmployeeId!.Value,
        TagId = tag.Id,
        ReaderId = dto.ReaderId ?? "unknown",
        Action = nextAction,
        OccurredAtUtc = now
    };

    db.PunchEvents.Add(evt);
    await db.SaveChangesAsync();
    return Results.Created($"/punches/{evt.Id}", evt);
});

// History
app.MapGet("/employees/{id:int}/punches", async (AppDbContext db, int id, DateTimeOffset? from, DateTimeOffset? to) =>
{
    var q = db.PunchEvents.AsQueryable().Where(p => p.EmployeeId == id);
    if (from.HasValue) q = q.Where(p => p.OccurredAtUtc >= from.Value);
    if (to.HasValue)   q = q.Where(p => p.OccurredAtUtc <= to.Value);
    var rows = await q.OrderByDescending(p => p.OccurredAtUtc).Take(500).ToListAsync();
    return Results.Ok(rows);
});

app.Run();
