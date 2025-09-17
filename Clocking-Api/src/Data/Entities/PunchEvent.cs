namespace Data.Entities;

public class PunchEvent
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int TagId { get; set; }
    public string ReaderId { get; set; } = "unknown"; // device/location
    public string Action { get; set; } = "in";        // "in" | "out"
    public DateTimeOffset OccurredAtUtc { get; set; }
    public Employee? Employee { get; set; }
    public NfcTag? Tag { get; set; }
}
