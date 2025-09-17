using System.ComponentModel.DataAnnotations;

namespace Clocking.Api.Models;

public class Worker
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    [Required] public string FullName { get; set; } = "";
    [Required] public string Department { get; set; } = "";
    public string? Role { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Nfccredential> Credentials { get; set; } = [];
    public List<WorkSession> Sessions { get; set; } = [];
}

public class Nfccredential
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public Worker? Worker { get; set; }
    [Required] public string UidHex { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
}
