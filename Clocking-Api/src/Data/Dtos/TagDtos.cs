namespace Data.Dtos;
public record CreateTagDto(string Uid, string? Nickname);
public record AssignTagDto(string Uid, int EmployeeId);
