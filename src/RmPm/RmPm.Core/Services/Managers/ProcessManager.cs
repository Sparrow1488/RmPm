using System.Diagnostics;
using RmPm.Core.Contracts;
using RmPm.Core.Services.Managers.Arguments;
using RmPm.Core.Services.Managers.Behaviours;

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

    private static async Task<string?> RunAsync(ProcessRunInfo runInfo, CancellationToken ctk = default)
    {
        var info = MapInfo(runInfo);
        
        using var process = new Process { StartInfo = info };

        var behaviours = new List<IExecutionBehaviour>
        {
            new CancellationExecutionBehaviour(ctk),
            new TimeoutExecutionBehaviour(runInfo.Timeout)
        };
        
        // Initialize
        behaviours.ForEach(x => x.Initialize(process));
        
        var result = await StartReadingAsync(process, ctk).ConfigureAwait(false);
        
        // Dispose
        await Task.WhenAll(behaviours.Select(x => x.DisposeAsync().AsTask())).ConfigureAwait(false);

        return result;
    }

    private static ProcessStartInfo MapInfo(ProcessRunInfo info)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = info.FileName,
            Arguments = info.Arguments,
            CreateNoWindow = !info.ShowConsole,
            WorkingDirectory = AppContext.BaseDirectory
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
}