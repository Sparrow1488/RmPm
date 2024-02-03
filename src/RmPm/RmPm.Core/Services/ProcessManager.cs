using System.Diagnostics;
using RmPm.Core.Contracts;
using Serilog;

namespace RmPm.Core.Services;

public class ProcessManager : IProcessManager
{
    private const string LogContext = "[PM]";
    private readonly ILogger _logger;

    public ProcessManager(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task<string?> RunAsync(ProcessRunInfo runInfo, CancellationToken ctk = default)
    {
        var info = MapInfo(runInfo);
        
        using var process = new Process { StartInfo = info };
        
        await using var cancellation = ctk.Register(() => Kill(process));
        // await using var timeout = RegisterTimeout(runInfo.Timeout, () => Kill(process)); // TODO: FEATURE
        
        var result = await StartReadingAsync(process, ctk);
        
        await process.WaitForExitAsync(ctk).ConfigureAwait(false);
        
        cancellation.Unregister();
        // timeout?.Unregister();

        return result;
    }

    private static ProcessStartInfo MapInfo(ProcessRunInfo info)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = info.FileName,
            Arguments = info.Arguments,
            CreateNoWindow = !info.ShowConsole,
            WorkingDirectory = Directory.GetCurrentDirectory()
        };

        // HACK: Читать из потока можно, когда не включена консоль
        if (!info.ShowConsole)
        {
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
        }

        return startInfo;
    }

    private void Kill(Process process)
    {
        _logger.Debug("{ctx} Process killed", LogContext);
        process.Kill();
    }
    
    private static async Task<string> StartReadingAsync(Process process, CancellationToken ctk = default)
    {
        if (process.StartInfo.CreateNoWindow)
        {
            process.EnableRaisingEvents = true;
            process.Start();
            return await process.StandardOutput.ReadToEndAsync(ctk);
        }
        
        process.Start();
        await process.WaitForExitAsync(ctk);
        return string.Empty;
    }

    private CancellationTokenRegistration? RegisterTimeout(TimeSpan? timeout, Action timeoutAction)
    {
        _logger.Debug("{ctx} RegisterTimeout {span}", LogContext, timeout.ToString());
        
        if (timeout is null) return default;
        
        var src = new CancellationTokenSource(timeout.Value);
        var token = src.Token;
        
        return token.Register(() =>
        {
            _logger.Debug("{ctx} Process timed out", LogContext);
            timeoutAction.Invoke();
            src.Dispose();
        });
    }
}

public class ProcessRunInfo
{
    public bool ShowConsole { get; set; }
    public string? Arguments { get; set; }
    public required string FileName { get; init; }
    // public TimeSpan? Timeout { get; set; } // TODO: FEATURE
}