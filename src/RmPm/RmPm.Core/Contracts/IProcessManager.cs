using RmPm.Core.Services;

namespace RmPm.Core.Contracts;

public interface IProcessManager
{
    Task<string?> RunAsync(RunArgs args, CancellationToken ctk = default);
    Task<string?> RunAsync(ProcessRunInfo runInfo, CancellationToken ctk = default);
}