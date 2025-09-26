using Clocking.Api.Data;
using Clocking.Api.Extensions;
using Clocking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Clocking.Api.Features.Admin;

var builder = WebApplication.CreateBuilder(args);

// EF Core (SQLite by default; uses ConnectionStrings:Default from appsettings.json)
builder.Services.AddDbContext<AppDbContext>(opts =>
{
    var cs = builder.Configuration.GetConnectionString("Default")
             ?? "Data Source=clocking.db";
    opts.UseSqlite(cs);
});

// CORS (open for now; tighten for prod)
builder.Services.AddCors(o =>
    o.AddPolicy("dev", p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseCors("dev");

// Auto-apply migrations on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Workers.Any())
    {
        db.Workers.Add(new Worker
        {
            FullName = "Test Worker",
            TagUid   = "04AABBCCDD22",
            IsActive = true               // <- REQUIRED
        });

        db.Readers.Add(new Reader
        {
            Code     = "LAB-001",
            Name     = "LAB-001",
            IsActive = true,
            Type     = default
        });

        db.SaveChanges();
    }
}


// Health checks
app.MapGet("/__debug/workers", async (AppDbContext db) =>
    Results.Ok(await db.Workers
        .Select(w => new { w.Id, w.FullName, w.TagUid, w.IsActive })
        .ToListAsync()));


// Feature endpoints (requires Extensions/EndpointRouteBuilderExtensions.cs)
app.MapApiEndpoints();

if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

await using (var scope = app.Services.CreateAsyncScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "AppData"));

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Readers.AnyAsync(r => r.Code == "LAB-001"))
        db.Readers.Add(new Reader { Code = "LAB-001", Name = "Lab door" });

    if (!await db.Workers.AnyAsync(w => w.TagUid == "04AABBCCDD22"))
        db.Workers.Add(new Worker { FullName = "Demo Worker", TagUid = "04AABBCCDD22" });

    await db.SaveChangesAsync();
}

app.MapAdminApi();

app.Run();

public partial class Program { }