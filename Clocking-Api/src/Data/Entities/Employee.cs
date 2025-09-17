namespace Data.Entities;

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public List<NfcTag> Tags { get; set; } = new();
    public List<PunchEvent> Punches { get; set; } = new();
}
