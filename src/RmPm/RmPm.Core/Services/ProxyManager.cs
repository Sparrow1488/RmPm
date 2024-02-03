using RmPm.Core.Contracts;
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
    protected async Task BashAsync(string command, TimeSpan? timeout = default, CancellationToken ctk = default)
    {
        const string logContext = "[Bash]";
        
        Logger.Debug("{ctx} " + command, logContext);

        using var src = timeout is null ? new CancellationTokenSource() : new CancellationTokenSource(timeout.Value);
        
        try
        {
            await ProcessManager.RunAsync(new ProcessRunInfo
            {
                Arguments = $"-c \"{command}\"",
                FileName = "/bin/bash",
                ShowConsole = false,
                // Timeout = timeout // TODO: FEATURE
            }, src.Token);
        }
        catch
        {
            Logger.Debug("{ctx} Timed out", logContext);
        }
    }

    public abstract Task<ProxyClient> CreateClientAsync(CreateRequest request, CancellationToken ctk = default);
}