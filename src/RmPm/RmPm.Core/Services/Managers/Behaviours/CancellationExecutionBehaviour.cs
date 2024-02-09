using System.Diagnostics;

namespace RmPm.Core.Services.Managers.Behaviours;

internal class CancellationExecutionBehaviour : IExecutionBehaviour
{
    private readonly CancellationToken _ctk;
    private CancellationTokenRegistration? _cancellation;

    public CancellationExecutionBehaviour(CancellationToken ctk)
    {
        _ctk = ctk;
    }
    
    public void Initialize(Process process)
    {
        _cancellation = _ctk.Register(process.Kill);
    }
    
    public ValueTask DisposeAsync()
    {
        _cancellation?.Unregister();
        return _cancellation?.DisposeAsync() ?? ValueTask.CompletedTask;
    }
}