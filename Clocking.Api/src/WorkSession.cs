namespace Clocking.Api.Models;

public class WorkSession
{
    public long Id { get; set; }
    public int WorkerId { get; set; }
    public Worker? Worker { get; set; }
    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public int? ReaderInId { get; set; }
    public Reader? ReaderIn { get; set; }
    public int? ReaderOutId { get; set; }
    public Reader? ReaderOut { get; set; }

    public DateTimeOffset CheckInAt { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
    public int? DurationSec { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.open;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
