namespace Clocking.Api.Data.Dtos;

/// <summary>
/// Read model for a scan (raw NFC tap) returned to clients.
/// </summary>
public record ScanDto
{
    /// <summary>Database identifier of the scan event.</summary>
    public int Id { get; init; }

    /// <summary>Worker who performed the scan.</summary>
    public int WorkerId { get; init; }
    public string WorkerName { get; init; } = string.Empty;

    /// <summary>Reader device (optional if unknown).</summary>
    public int? ReaderId { get; init; }
    public string? ReaderCode { get; init; }
    public string? ReaderName { get; init; }

    /// <summary>The NFC tag UID captured at scan time (uppercase hex, if available).</summary>
    public string? Uid { get; init; }

    /// <summary>Timestamp of the scan in UTC.</summary>
    public DateTimeOffset OccurredAtUtc { get; init; }
}
