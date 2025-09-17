namespace Clocking.Api.Data.Entities;

/// <summary>
/// Direction of a clocking action derived from a scan.
/// Useful if you log explicit punch events in addition to WorkSessions.
/// </summary>
public enum WorkAction
{
    In  = 1,
    Out = 2
}

/// <summary>
/// State of a work session; Open when EndUtc is null, Closed otherwise.
/// </summary>
public enum WorkSessionState
{
    Open   = 1,
    Closed = 2
}

/// <summary>
/// What kind of reader device generated the scan.
/// </summary>
public enum ReaderType
{
    Fixed   = 1,   // e.g., wall-mounted reader
    Mobile  = 2,   // e.g., USB reader on a laptop
    Virtual = 3    // e.g., API-created or simulated
}

/// <summary>
/// Where a scan/punch originated from.
/// </summary>
public enum ScanOrigin
{
    Nfc   = 1,   // NFC hardware (default)
    Api   = 2,   // direct API call
    Admin = 3    // backoffice/manual
}
