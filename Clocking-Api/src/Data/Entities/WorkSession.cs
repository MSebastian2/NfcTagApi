using System.ComponentModel.DataAnnotations.Schema;

namespace Clocking.Api.Data.Entities;

public class WorkSession
{
    public int Id { get; set; }

    /// <summary>Worker this session belongs to.</summary>
    public int WorkerId { get; set; }
    public Worker? Worker { get; set; }

    /// <summary>Clock-in timestamp (UTC).</summary>
    public DateTimeOffset StartUtc { get; set; }

    /// <summary>Clock-out timestamp (UTC). Null while session is open.</summary>
    public DateTimeOffset? EndUtc { get; set; }

    /// <summary>Reader used to start the session (optional).</summary>
    public int? StartReaderId { get; set; }
    public Reader? StartReader { get; set; }

    /// <summary>Reader used to end the session (optional).</summary>
    public int? EndReaderId { get; set; }
    public Reader? EndReader { get; set; }

    /// <summary>Convenience: derived state (not mapped).</summary>
    [NotMapped]
    public WorkSessionState State => EndUtc is null ? WorkSessionState.Open : WorkSessionState.Closed;

    /// <summary>Convenience: duration when closed (not mapped).</summary>
    [NotMapped]
    public TimeSpan? Duration => EndUtc is null ? null : EndUtc.Value - StartUtc;
}
