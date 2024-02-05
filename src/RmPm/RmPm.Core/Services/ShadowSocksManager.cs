using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RmPm.Core.Configuration;
using RmPm.Core.Contracts;
using RmPm.Core.Models;
using Serilog;

namespace RmPm.Core.Services;

public class ShadowSocksManager : ProxyManager
{
    private const string LogContext = "ss-manager";
    
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerSettings _serializeSettings;
    private readonly NetStat _netStats;

    public ShadowSocksManager(IConfiguration configuration, IProcessManager pm, ILogger logger) : base(pm, logger)
    {
        _configuration = configuration;
        _serializeSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
        _netStats = new NetStat(pm, logger);
    }

    public async Task<ProxySession[]> GetSessionsAsync(CancellationToken ctk = default)
    {
        var configs = await GetConfigsAsync(ctk);
        
        var items = await _netStats.GetListenersAsync();
        var ssListeners = items.Where(x => x.ProgramName.StartsWith("ss-")).ToArray();
        
        Logger.Debug("[{ctx}] Found {count} ss listeners", LogContext, ssListeners.Length);

        var sessions = new List<ProxySession>();

        foreach (var listener in ssListeners)
        {
            var address = IPv4Address.Parse(listener.LocalAddress);
            
            if (address is null)
            {
                Logger.Debug("[{ctx}] Invalid listener ({pid}) address {value}", LogContext, listener.Pid, listener.LocalAddress);
                continue;
            }
            
            var matchConfig = configs.FirstOrDefault(c => c.Config.ServerPort == address.Value.Port);

            if (matchConfig == default)
            {
                Logger.Debug("[{ctx}] Config for listener {address} no match", LogContext, listener.LocalAddress);
                continue;                
            }
            
            sessions.Add(new ProxySession(listener.LocalAddress, matchConfig.Config, listener));
        }
        
        return sessions.ToArray();
    }

    public override async Task<ProxyClient> CreateClientAsync(CreateRequest request, CancellationToken ctk = default)
    {
        var (config, json) = GenConfig(request.Method);
        var directory = _configuration["Proxies:ShadowSocks:ConfigsDir"]!;
        
        RequireDirectory(directory);
        await ActivateAsync(await SaveConfigAsync(directory, json, ctk)).ConfigureAwait(false);
        
        return new ProxyClient(request.Name, config, json, EncodeInline(config));
    }
    
    private ConfigTuple GenConfig(string method)
    {
        // IConfigGenerator.Create
        
        var config = new SocksConfig(
            _configuration["Server:Ip"]!,
            Random.Shared.Next(2000, 9000),
            Random.Shared.Next(2000, 9000),
            Guid.NewGuid().ToString().Replace("-", ""),
            method
        );
        var json = JsonConvert.SerializeObject(config, _serializeSettings);
        
        return new ConfigTuple(config, json);
    }

    private static void RequireDirectory(string directory)
    {
       if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
    }

    private async Task<string> SaveConfigAsync(string dir, string config, CancellationToken ctk = default)
    {
        // TODO: IConfigProvider.Save
        
        var savePath = Path.Combine(dir, GetConfigName());
        await File.WriteAllTextAsync(savePath, config, ctk).ConfigureAwait(false);
        Logger.Debug("[{ctx}] Config saved {status}, path '{path}'", LogContext, File.Exists(savePath) ? "success" : "failed", savePath);

        return savePath;
    }

    private async Task ActivateAsync(string configPath)
    {
        var command = $"ss-server -c {configPath} & ";
        await BashAsync(command, timeout: TimeSpan.FromSeconds(2));
    }

    private static string EncodeInline(SocksConfig config)
    {
        var line = config.Method + ":" + config.Password + "@" + config.Server + ":" + config.ServerPort;
        return "ss://" + Convert.ToBase64String(Encoding.UTF8.GetBytes(line)) + "\n";
    }

    private string GetConfigName()
    {
        // TODO: IConfigProvider.ConfigNamingStrategy
        
        var configLastNumber = GetAllConfigFiles()
            .Select(Path.GetFileNameWithoutExtension)
            .Select(x => x!.ToArray().SkipWhile(y => !char.IsDigit(y)))
            .Select(x => string.Join("", x))
            .Where(x => x.All(char.IsDigit) && !string.IsNullOrWhiteSpace(x))
            .Select(int.Parse)
            .Max();
        
        return "config" + (configLastNumber == 0 ? 1 : ++configLastNumber) + ".json";
    }

    private string[] GetAllConfigFiles()
    {
        // TODO: IConfigProvider.GetAllPath
        return Directory.GetFiles(_configuration["Proxies:ShadowSocks:ConfigsDir"]!)
            .Where(x => Path.GetFileNameWithoutExtension(x).Any(char.IsDigit))
            .ToArray();
    }

    private async Task<ConfigTuple[]> GetConfigsAsync(CancellationToken ctk = default)
    {
        // TODO: IConfigProvider.GetAll
        
        var files = GetAllConfigFiles();
        var result = new List<ConfigTuple>();

        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, ctk);
            var config = JsonConvert.DeserializeObject<SocksConfig>(json, _serializeSettings);

            if (config is null)
                throw new InvalidOperationException($"Failed to deserialize config {file}");
            
            result.Add(new ConfigTuple(config, json));
        }

        return result.ToArray();
    }

    private readonly record struct ConfigTuple(SocksConfig Config, string ConfigJson);
}