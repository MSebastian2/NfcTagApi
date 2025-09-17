namespace Data.Entities;

public class NfcTag
{
    public int Id { get; set; }
    public string Uid { get; set; } = string.Empty; // uppercase hex
    public string? Nickname { get; set; }
    public bool IsActive { get; set; } = true;

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}
