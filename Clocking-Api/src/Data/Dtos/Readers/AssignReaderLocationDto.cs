namespace Clocking.Api.Data.Dtos;

/// <summary>
/// Assigns a reader to a location. You must provide ReaderCode, and either LocationId or LocationCode.
/// </summary>
public record AssignReaderLocationDto
{
    /// <summary>Unique code of the reader device (e.g., "ACR122U-LAB").</summary>
    public string ReaderCode { get; init; } = string.Empty;

    /// <summary>Target location by numeric ID (preferred for internal ops).</summary>
    public int? LocationId { get; init; }

    /// <summary>Alternative: target location by code (e.g., "HQ").</summary>
    public string? LocationCode { get; init; }
}
