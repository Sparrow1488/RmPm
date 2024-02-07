using Microsoft.Extensions.Configuration;
using RmPm.Core.Configuration;
using RmPm.Core.Contracts;
using RmPm.Core.Models;
using Serilog;

namespace RmPm.Core.Services.Socks;

public class SocksConfigProvider : IConfigFileProvider<SocksConfig>
{
    private const string DefaultConsistentName = "config";
    private const int DefaultFirstServerPort = 8130;
    private const int DefaultFirstLocalPort = 2020;
    private const int GenerationPortStep = 1;
    
    private readonly string _root;
    private readonly SocksConfigReader _configReader;
    private readonly Store _store;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public SocksConfigProvider(
        IConfiguration configuration, 
        SocksConfigReader configReader,
        Store store,
        ILogger logger,
        Func<ConsistentNamingStrategy.FileIndex, bool>? filesFilter = default)
    {
        var consistentName = configuration.GetValue<string>(ConfigNames.ShadowSocks.ConsistentName) ?? DefaultConsistentName;
        NamingStrategy = new ConsistentNamingStrategy(consistentName, filesFilter);
        
        _logger = logger;
        _configReader = configReader;
        _store = store;
        _configuration = configuration;
        _root = configuration.GetValue<string>(ConfigNames.ShadowSocks.ConfigsRoot)!;
        
        RequireRoot(_root);
    }

    private FileNamingStrategy NamingStrategy { get; }
    
    public async Task<string> SaveAsync(SocksConfig config, CancellationToken ctk = default)
    {
        var json = _configReader.ToJson(config);
        var filename = NamingStrategy.GetNewName(SocksConfig.Extension, GetRootFiles());
        var savePath = Path.Combine(_root, filename);

        if (File.Exists(savePath))
            throw new InvalidOperationException($"Cannot save already exists file {savePath}");
        
        _logger.Debug("Config write in '{path}'", savePath);

        await File.WriteAllTextAsync(savePath, json, ctk);

        var entry = new EntryStore
        {
            ConfigPath = savePath,
            FriendlyName = config.FriendlyName
        };

        if (string.IsNullOrWhiteSpace(entry.FriendlyName))
            entry.SetFriendlyNameByFilename(savePath);
        
        await _store.SaveAsync(entry, ctk);
        
        return config.FilePath = savePath;
    }

    public async Task DeleteAsync(SocksConfig config, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(config.FilePath) && !File.Exists(config.FilePath))
        {
            _logger.Information("Config not found and cannot be deleted");
        }
        else
        {
            File.Delete(config.FilePath);
        }

        var entry = await _store.FindAsync(x => x.ConfigPath == config.FilePath, ctk);
        if (entry is not null)
            await _store.DeleteAsync(entry, ctk);
    }

    public async Task<SocksConfig> GenerateAsync(CancellationToken ctk = default)
    {
        var all = await GetAllAsync(ctk);
        var serverPort = DefaultFirstServerPort;
        var localPort = DefaultFirstLocalPort;

        // ReSharper disable once InvertIf
        if (all.Length != 0)
        {
            serverPort = all.Max(x => x.ServerPort) + GenerationPortStep;
            localPort = all.Max(x => x.LocalPort) + GenerationPortStep;
        }

        return new SocksConfig(
            _configuration[ConfigNames.ServerIp]!,
            serverPort,
            localPort,
            Guid.NewGuid().ToString().Replace("-", ""),
            _configuration[ConfigNames.ShadowSocks.GenMethod] ?? Encrypt.ShadowSocks.Default
        );
    }

    public async Task<SocksConfig[]> GetAllAsync(CancellationToken ctk = default)
    {
        var files = NamingStrategy.SelectFiles(GetRootFiles());
        var entries = await _store.GetAllAsync(ctk);
        var result = new List<SocksConfig>();

        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, ctk);
            var config = _configReader.ToConfig(json);
            
            if (config == default)
                _logger.Warning("Failed to deserialize config {file}", file);
            else
                result.Add(AttachAdditionalInfo(config, file, entries));
        }

        return result.ToArray();
    }

    private static SocksConfig AttachAdditionalInfo(SocksConfig config, string savePath, EntryStore[] entries)
    {
        var friendlyName = entries.FirstOrDefault(x => x.ConfigPath == savePath)?.FriendlyName;
        config.FilePath = savePath;
        config.FriendlyName = friendlyName;
        
        return config;
    }
    
    private string[] GetRootFiles() => Directory.GetFiles(_root);

    private static void RequireRoot(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path), "Passed configs root path is null or empty");
        
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException("Configs root path not exists");
    }
}