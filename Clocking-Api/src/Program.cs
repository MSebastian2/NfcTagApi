using Clocking.Api.Data;
using Clocking.Api.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core (SQLite by default; uses ConnectionStrings:Default from appsettings.json)
builder.Services.AddDbContext<AppDbContext>(opts =>
{
    var cs = builder.Configuration.GetConnectionString("Default")
             ?? "Data Source=clocking.db";
    opts.UseSqlite(cs);
});

// CORS (open for now; tighten for prod)
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

// Auto-apply migrations on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Health check
app.MapGet("/ping", () => Results.Ok(new { ok = true, at = DateTimeOffset.UtcNow }));

// Feature endpoints (requires Extensions/EndpointRouteBuilderExtensions.cs)
app.MapApiEndpoints();

app.Run();
