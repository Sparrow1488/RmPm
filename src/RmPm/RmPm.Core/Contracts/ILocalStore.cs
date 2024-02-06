namespace RmPm.Core.Contracts;

public interface ILocalStore
{
    bool Exists(string filename);
    Task WriteAsync(string filename, string content, CancellationToken ctk = default);
    Task<string> ReadAsync(string filename, CancellationToken ctk = default);
}