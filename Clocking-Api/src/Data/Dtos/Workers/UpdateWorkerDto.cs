namespace Clocking.Api.Data.Dtos;

/// <summary>
/// Partial update for a worker. Only non-null properties will be applied.
/// </summary>
public record UpdateWorkerDto
{
    /// <summary>New full name (leave null to keep current).</summary>
    public string? FullName { get; init; }

    /// <summary>New NFC tag UID (uppercase hex). Leave null to keep current; set empty string to clear.</summary>
    public string? TagUid { get; init; }

    /// <summary>Activate/deactivate the worker. Leave null to keep current.</summary>
    public bool? IsActive { get; init; }
}
