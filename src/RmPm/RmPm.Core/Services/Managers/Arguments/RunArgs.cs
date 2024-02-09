namespace RmPm.Core.Services.Managers.Arguments;

public record RunArgs(string FileName, string Arguments, bool ReadOutputOrHideConsole = false, TimeSpan? Timeout = null);

public class ProcessRunInfo
{
    public bool ShowConsole { get; init; }
    public string? Arguments { get; init; }
    public required string FileName { get; init; }
    public TimeSpan? Timeout { get; init; }
}