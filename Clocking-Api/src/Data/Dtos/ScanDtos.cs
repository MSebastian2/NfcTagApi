namespace Clocking.Api.Data.Dtos;

public record ScanRequestDto(string TagUid, string? ReaderCode);
public record ScanResultDto(bool created, bool ignored, string action, DateTimeOffset at);
