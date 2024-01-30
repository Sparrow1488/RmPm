using System.Diagnostics;

namespace RmPm.Core.Services;

/// <summary>
/// Утилита для управления прокси или ВПН
/// </summary>
public abstract class ProxyManager
{
    /// <summary>
    /// Выполнить команду в терминале
    /// </summary>
    /// <returns></returns>
    protected static Task BashAsync(string command, CancellationToken ctk = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);

        if (process is null)
            throw new InvalidOperationException("Failed to run bash command " + command);

        return process.WaitForExitAsync(ctk);
    }

    public abstract Task<ProxyClient> CreateClientAsync(CreateRequest request, CancellationToken ctk = default);
}