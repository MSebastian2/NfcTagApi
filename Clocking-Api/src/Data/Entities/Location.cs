namespace Clocking.Api.Data.Entities;

public class Location
{
    public int Id { get; set; }

    /// <summary>Human-friendly name (e.g., "Headquarters").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional short code (e.g., "HQ").</summary>
    public string? Code { get; set; }

    /// <summary>Whether this location is active/usable.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Readers physically/logically assigned to this location.</summary>
    public List<Reader> Readers { get; set; } = new();
}
