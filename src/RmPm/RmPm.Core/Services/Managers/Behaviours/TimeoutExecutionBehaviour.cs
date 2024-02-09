using System.Diagnostics;

namespace RmPm.Core.Services.Managers.Behaviours;

internal class TimeoutExecutionBehaviour : IExecutionBehaviour
{
    private readonly TimeSpan? _timeout;
    private CancellationTokenSource? _timeoutSource;

    public TimeoutExecutionBehaviour(TimeSpan? timeout)
    {
        _timeout = timeout;
    }
    
    public void Initialize(Process process)
    {
        if (_timeout == default) return;
        
        _timeoutSource = RegisterTimeout(_timeout, process.Kill);
    }

    private static CancellationTokenSource? RegisterTimeout(TimeSpan? timeout, Action timeoutAction)
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
    
    public ValueTask DisposeAsync()
    {
        _timeoutSource?.Dispose();
        return ValueTask.CompletedTask;
    }
}