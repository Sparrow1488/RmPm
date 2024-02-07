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
    protected ProxyManager(IProcessManager processManager)
    {
        ProcessManager = processManager;
    }
    
    protected IProcessManager ProcessManager { get; }

    /// <summary>
    /// Выполнить команду в терминале
    /// </summary>
    /// <returns></returns>
    protected async Task BashAsync(BashRun args)
    {
        TimeSpan? timeout = null;

        if (args is BashRunDetached)
            timeout = TimeSpan.FromSeconds(0.5);
        
        using var src = timeout is null ? new CancellationTokenSource() : new CancellationTokenSource(timeout.Value);
        
        try
        {
            await ProcessManager.RunAsync(args, src.Token);
        }
        catch
        {
            // Ignore
        }
    }

    public abstract Task<ProxyClientConfig> CreateClientAsync(CreateClientRequest request, CancellationToken ctk = default);
}