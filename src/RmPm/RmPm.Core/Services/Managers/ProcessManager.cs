using System.Diagnostics;
using RmPm.Core.Contracts;

namespace RmPm.Core.Services.Managers;

public class ProcessManager : IProcessManager
{
    public Task<string?> RunAsync(RunArgs args, CancellationToken ctk = default)
    {
        var runInfo = new ProcessRunInfo
        {
            FileName = args.FileName,
            Arguments = args.Arguments,
            ShowConsole = !args.ReadOutputOrHideConsole,
            Timeout = args.Timeout
        };

        return RunAsync(runInfo, ctk);
    }

    public async Task<string?> RunAsync(ProcessRunInfo runInfo, CancellationToken ctk = default)
    {
        var info = MapInfo(runInfo);
        
        using var process = new Process { StartInfo = info };
        
        await using var cancellation = ctk.Register(() => Kill(process));
        using var timeout = RegisterTimeout(runInfo.Timeout, () => Kill(process));
        
        var result = await StartReadingAsync(process, ctk).ConfigureAwait(false);
        
        await process.WaitForExitAsync(ctk).ConfigureAwait(false);
        
        cancellation.Unregister();

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

    private static void Kill(Process process)
    {
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

    private CancellationTokenSource? RegisterTimeout(TimeSpan? timeout, Action timeoutAction)
    {
        if (timeout is null) return default;
        
        var src = new CancellationTokenSource(timeout.Value);
        var token = src.Token;
        
        token.Register(() =>
        {
            timeoutAction.Invoke();
            src.Dispose();
        });

        return src;
    }
}

public class ProcessRunInfo
{
    public bool ShowConsole { get; set; }
    public string? Arguments { get; set; }
    public required string FileName { get; init; }
    public TimeSpan? Timeout { get; set; }
}

public record RunArgs(string FileName, string Arguments, bool ReadOutputOrHideConsole = false, TimeSpan? Timeout = null);

// internal interface IExecutionBehaviour : IDisposable
// {
//     void Initialize(Process process);
// }
//
// internal class CancellationExecutionBehaviour : IExecutionBehaviour
// {
//     private readonly CancellationToken _ctk;
//
//     public CancellationExecutionBehaviour(CancellationToken ctk)
//     {
//         _ctk = ctk;
//     }
//     
//     public void Initialize(Process process)
//     {
//         _ctk = 
//     }
//     
//     public void Dispose()
//     {
//         throw new NotImplementedException();
//     }
// }