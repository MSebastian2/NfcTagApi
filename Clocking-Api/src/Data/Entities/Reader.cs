namespace Clocking.Api.Data.Entities;

public class Reader
{
    public int Id { get; set; }

    /// <summary>Unique device code (e.g., "ACR122U-LAB").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Human-friendly name (e.g., "Lab Door").</summary>
    public string? Name { get; set; }

    /// <summary>Whether this reader is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Type of reader device.</summary>
    public ReaderType Type { get; set; } = ReaderType.Fixed;

    /// <summary>Optional location link.</summary>
    public int? LocationId { get; set; }
    public Location? Location { get; set; }
}
