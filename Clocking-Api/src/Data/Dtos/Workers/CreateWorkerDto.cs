namespace Clocking.Api.Data.Dtos;

/// <summary>
/// Payload to create a worker/employee. TagUid is optional at creation time
/// and can be assigned later or updated via a separate endpoint.
/// </summary>
public record CreateWorkerDto
{
    /// <summary>Full legal name or display name.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>NFC tag UID bound to this worker (uppercase hex), optional.</summary>
    public string? TagUid { get; init; }

    /// <summary>Whether the worker is active upon creation.</summary>
    public bool IsActive { get; init; } = true;
}
