namespace Clocking.Api.Models;

public class Scan
{
    public long Id { get; set; }
    public int ReaderId { get; set; }
    public Reader? Reader { get; set; }
    public string UidHex { get; set; } = "";
    public DateTimeOffset ScannedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeviceTime { get; set; }
    public string? IpAddress { get; set; }
    public string? IdempotencyKey { get; set; }
}
