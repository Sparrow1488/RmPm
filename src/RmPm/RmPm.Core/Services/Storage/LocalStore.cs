using RmPm.Core.Contracts;

namespace RmPm.Core.Services.Storage;

public class LocalStore : ILocalStore
{
    private readonly string _appPath;

    public LocalStore(string appPath)
    {
        _appPath = appPath;
    }

    public bool Exists(string filename) => File.Exists(FullPath(filename));

    public Task WriteAsync(string filename, string content, CancellationToken ctk = default)
    {
        return File.WriteAllTextAsync(FullPath(filename), content, ctk);
    }

    public Task<string> ReadAsync(string filename, CancellationToken ctk = default)
    {
        if (!Exists(filename))
            return Task.FromResult(string.Empty);
        
        return File.ReadAllTextAsync(FullPath(filename), ctk);
    }

    private string FullPath(string filename) => Path.Combine(_appPath, filename);
}