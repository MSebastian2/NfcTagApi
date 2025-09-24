namespace Clocking.Api.Data.Entities;

public class Scan
{
    public int Id { get; set; }

    /// <summary>Worker who performed the scan.</summary>
    public int WorkerId { get; set; }
    public Worker Worker { get; set; } = null!;

    /// <summary>Reader device that captured the scan (optional).</summary>
    public int ReaderId { get; set; }
    public Reader Reader { get; set; } = null!;

    /// <summary>NFC tag UID read at the moment of the scan (uppercase hex, if available).</summary>
    public string? Uid { get; set; }

    /// <summary>When the scan happened (UTC).</summary>
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTime WhenUtc { get; set; }

    /// <summary>Source of the scan (defaults to hardware NFC).</summary>
    public ScanOrigin Origin { get; set; } = ScanOrigin.Nfc;
    public ScanType Type { get; set; } = ScanType.Unknown;
}
