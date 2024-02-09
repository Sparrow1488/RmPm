using RmPm.Core.Services.Managers.Arguments;

namespace RmPm.Core.Contracts;

public interface IProcessManager
{
    Task<string?> RunAsync(RunArgs args, CancellationToken ctk = default);
}