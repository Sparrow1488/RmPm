using Microsoft.Extensions.Configuration;
using RmPm.Core.Configuration;
using RmPm.Core.Contracts;

namespace RmPm.Core.Services.Socks;

public class SocksConfigProvider : IConfigFileProvider<SocksConfig>
{
    private const string DefaultConsistentName = "config";
    
    private readonly string _root;
    private readonly IJsonService _jsonService;

    public SocksConfigProvider(IConfiguration configuration, IJsonService jsonService)
    {
        _jsonService = jsonService;
        _root = configuration.GetValue<string>(ConfigNames.ShadowSocks.ConfigsRoot)!;
        var consistentName = configuration.GetValue<string>(ConfigNames.ShadowSocks.ConsistentName) ?? DefaultConsistentName;

        // NOTE: ставим фильтр на конфиг без номера, потому что это конфиг сервера, а не клиентов
        NamingStrategy = new ConsistentNamingStrategy(consistentName, i => i.Number is not null);
        
        RequireRoot(_root);
    }

    public FileNamingStrategy NamingStrategy { get; }

    public Task<string[]> GetFilesAsync(CancellationToken ctk = default)
    {
        var rootFiles = Directory.GetFiles(_root);
        return Task.FromResult(NamingStrategy.SelectFiles(rootFiles));
    }

    public async Task<SocksConfig[]> GetAllAsync(CancellationToken ctk = default)
    {
        var files = await GetFilesAsync(ctk);
        var result = new List<SocksConfig>();

        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, ctk);
            var config = _jsonService.Deserialize<SocksConfig>(json);

            if (config == default)
                throw new InvalidOperationException($"Failed to deserialize config {file}");
            
            result.Add(config);
        }

        return result.ToArray();
    }

    private static void RequireRoot(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path), "Passed configs root path is null or empty");
        
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException("Configs root path not exists");
    }
}