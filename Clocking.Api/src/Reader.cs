using System.ComponentModel.DataAnnotations;

namespace Clocking.Api.Models;

public class Reader
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = "";
    public int LocationId { get; set; }
    public Location? Location { get; set; }
    [Required] public string ApiKey { get; set; } = ""; // dev: plain; prod: hash this
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastSeenAt { get; set; }

    public List<Scan> Scans { get; set; } = [];
    public List<WorkSession> SessionsIn { get; set; } = [];
    public List<WorkSession> SessionsOut { get; set; } = [];
}
