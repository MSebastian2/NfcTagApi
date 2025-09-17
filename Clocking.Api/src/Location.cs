using System.ComponentModel.DataAnnotations;

namespace Clocking.Api.Models;

public class Location
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = "";
    public string? Address { get; set; }
    public LocationType Type { get; set; } = LocationType.office;
    public bool IsActive { get; set; } = true;

    public List<Reader> Readers { get; set; } = [];
    public List<WorkSession> Sessions { get; set; } = [];
}
