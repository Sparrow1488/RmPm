using RmPm.Core.Contracts;
using RmPm.Core.Services;

namespace RmPm.Core.Extensions;

public static class ProcessManagerExtensions
{
    public static Task<string?> BashAsync(this IProcessManager pm, string command, CancellationToken ctk = default)
    {
        return pm.RunAsync(new ProcessRunInfo
        {
            Arguments = $"-c \"{command}\"",
            FileName = "/bin/bash",
            ShowConsole = false
        }, ctk);
    }
}