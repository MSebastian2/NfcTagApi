using Clocking.Api.Features.Scans;
using Clocking.Api.Features.Workers;
using Clocking.Api.Features.Readers;
using Clocking.Api.Features.Reports;

namespace Clocking.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Central place to register all feature endpoints.
    /// Call this from Program.cs via app.MapApiEndpoints();
    /// </summary>
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Always available (you created Features/Scans/Endpoints.cs)
        app.MapScanEndpoints();

        // Uncomment these as you add the corresponding feature files:
        // app.MapWorkerEndpoints();
        // app.MapReaderEndpoints();
        // app.MapReportEndpoints();

        return app;
    }
}
