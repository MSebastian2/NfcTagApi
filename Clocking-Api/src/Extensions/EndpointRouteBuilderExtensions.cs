using Clocking.Api.Features.Readers;
using Clocking.Api.Features.Reports;
using Clocking.Api.Features.Scans;
using Clocking.Api.Features.Workers;

namespace Clocking.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapWorkerEndpoints();
        app.MapReaderEndpoints();
        app.MapScanEndpoints();
        app.MapReportEndpoints();
        return app;
    }
}
