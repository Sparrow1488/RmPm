using System.Diagnostics;

namespace RmPm.Core.Services.Managers.Behaviours;

internal interface IExecutionBehaviour : IAsyncDisposable
{
    void Initialize(Process process);
}