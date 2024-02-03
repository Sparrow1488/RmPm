using RmPm.Core.Services;

namespace RmPm.Core.Contracts;

public interface IProcessManager
{
    Task<string?> RunAsync(ProcessRunInfo runInfo, CancellationToken ctk = default);
}