namespace Clocking.Api.Data.Entities;

public class Worker
{
    public int Id { get; set; }

    /// <summary>Full name of the employee.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>NFC tag UID bound to this worker (uppercase hex). Optional.</summary>
    public string? TagUid { get; set; }

    /// <summary>Whether the worker is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Clocking sessions for this worker.</summary>
    public List<WorkSession> WorkSessions { get; set; } = new();

    /// <summary>Raw NFC scans made by this worker.</summary>
    public List<Scan> Scans { get; set; } = new();
}
