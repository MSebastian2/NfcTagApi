namespace Clocking.Api.Data.Dtos;

/// <summary>
/// Generic report range & filters for attendance/clocking reports.
/// Times are expected in UTC. If FromUtc/ToUtc are null, your endpoint can apply defaults
/// (e.g., last 30 days).
/// </summary>
public record ReportRangeDto
{
    /// <summary>Start of range (inclusive, UTC).</summary>
    public DateTimeOffset? FromUtc { get; init; }

    /// <summary>End of range (inclusive, UTC).</summary>
    public DateTimeOffset? ToUtc { get; init; }

    /// <summary>Optional: filter by worker.</summary>
    public int? WorkerId { get; init; }

    /// <summary>Optional: filter by location.</summary>
    public int? LocationId { get; init; }

    /// <summary>Optional: filter by reader device code (e.g., "ACR122U-LAB").</summary>
    public string? ReaderCode { get; init; }

    /// <summary>
    /// Optional grouping for summaries: "day" | "week" | "month".
    /// Leave empty for raw rows or endpoint-specific default.
    /// </summary>
    public string? GroupBy { get; init; } = "day";
}
