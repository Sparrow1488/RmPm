using RmPm.Core.Configuration;

namespace RmPm.Core.Contracts;

public interface IConfigFileProvider<TConfig> where TConfig : ProxyClientConfig
{
    FileNamingStrategy NamingStrategy { get; }
    Task<string[]> GetFilesAsync(CancellationToken ctk = default);
    Task<TConfig[]> GetAllAsync(CancellationToken ctk = default);
}