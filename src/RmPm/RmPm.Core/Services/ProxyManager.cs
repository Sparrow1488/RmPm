using RmPm.Core.Configuration;
using RmPm.Core.Contracts;
using RmPm.Core.Models;
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
    
    protected IProcessManager ProcessManager { get; }
    protected ILogger Logger { get; }

    /// <summary>
    /// Выполнить команду в терминале
    /// </summary>
    /// <returns></returns>
    protected async Task BashAsync(BashRun args)
    {
        const string logContext = "[Bash]";

        TimeSpan? timeout = null;

        if (args is BashRunDetached)
        {
            timeout = TimeSpan.FromSeconds(1);
        }
        
        Logger.Debug("{ctx} " + args.Arguments, logContext);

        using var src = timeout is null ? new CancellationTokenSource() : new CancellationTokenSource(timeout.Value);
        
        try
        {
            await ProcessManager.RunAsync(args, src.Token);
        }
        catch
        {
            Logger.Debug("{ctx} Timed out", logContext);
        }
    }

    public abstract Task<ProxyClientConfig> CreateClientAsync(CreateClientRequest request, CancellationToken ctk = default);
}