using RmPm.Core.Configuration;

namespace RmPm.Core.Contracts;

public interface IConfigFileProvider<TConfig> 
    where TConfig : ProxyClientConfig
{
    Task<string> SaveAsync(TConfig config, CancellationToken ctk = default);
    Task DeleteAsync(TConfig config, CancellationToken ctk = default);
    Task<TConfig> GenerateAsync(CancellationToken ctk = default);
    Task<TConfig[]> GetAllAsync(CancellationToken ctk = default);
}