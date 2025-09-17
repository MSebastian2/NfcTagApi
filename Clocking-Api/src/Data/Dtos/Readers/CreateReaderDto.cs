namespace Clocking.Api.Data.Dtos;

/// <summary>
/// Payload to create a reader device. Provide a unique Code; Name is optional.
/// You may optionally attach it to a location via LocationId or LocationCode.
/// </summary>
public record CreateReaderDto
{
    /// <summary>Unique code of the reader (e.g., "ACR122U-LAB").</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Human-friendly label (e.g., "Lab Door").</summary>
    public string? Name { get; init; }

    /// <summary>Whether the reader is active upon creation.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Optional: numeric location id to attach this reader to.</summary>
    public int? LocationId { get; init; }

    /// <summary>Optional: location code to attach this reader to (e.g., "HQ").</summary>
    public string? LocationCode { get; init; }
}
