using RmPm.Core.Contracts;
using RmPm.Core.Models;

namespace RmPm.Core.Extensions;

public static class BashExtensions
{
    public static Task KillAsync(this IProcessManager pm, string pid, CancellationToken ctk = default)
    {
        return pm.RunAsync(new BashKill(pid), ctk);
    }
}