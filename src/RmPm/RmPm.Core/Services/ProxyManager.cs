using RmPm.Core.Configuration;
using RmPm.Core.Contracts;
using RmPm.Core.Extensions;
using Serilog;

namespace RmPm.Core.Services;

/// <summary>
/// Утилита для управления прокси или ВПН
/// </summary>
public abstract class ProxyManager
{
    protected ProxyManager(IProcessManager processManager, ILogger logger)
    {
        ProcessManager = processManager;
        Logger = logger;
    }
    
    private IProcessManager ProcessManager { get; }
    protected ILogger Logger { get; }

    /// <summary>
    /// Выполнить команду в терминале
    /// </summary>
    /// <returns></returns>
    protected async Task BashAsync(string command, TimeSpan? timeout = default)
    {
        const string logContext = "[Bash]";
        
        Logger.Debug("{ctx} " + command, logContext);

        using var src = timeout is null ? new CancellationTokenSource() : new CancellationTokenSource(timeout.Value);
        
        try
        {
            await ProcessManager.BashAsync(command, src.Token);
        }
        catch
        {
            Logger.Debug("{ctx} Timed out", logContext);
        }
    }

    public abstract Task<ProxyClient> CreateClientAsync(CancellationToken ctk = default);
}