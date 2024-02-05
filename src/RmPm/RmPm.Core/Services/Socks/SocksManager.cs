using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RmPm.Core.Configuration;
using RmPm.Core.Contracts;
using RmPm.Core.Models;
using Serilog;

namespace RmPm.Core.Services.Socks;

public class SocksManager : ProxyManager
{
    private const string LogContext = "ss-manager";

    private readonly IConfigFileProvider<SocksConfig> _configProvider;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerSettings _serializeSettings;
    private readonly NetStat _netStats;

    public SocksManager(
        IConfigFileProvider<SocksConfig> configProvider,
        IProcessManager pm,
        IConfiguration configuration,
        ILogger logger
    ) : base(pm, logger)
    {
        _configProvider = configProvider;
        _configuration = configuration;
        _serializeSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
        _netStats = new NetStat(pm);
    }

    public async Task<ProxySession[]> GetSessionsAsync(CancellationToken ctk = default)
    {
        var configs = await _configProvider.GetAllAsync(ctk);
        
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
            
            var matchConfig = configs.FirstOrDefault(c => c.ServerPort == address.Value.Port);

            if (matchConfig == default)
            {
                Logger.Debug("[{ctx}] Config for listener {address} no match", LogContext, listener.LocalAddress);
                continue;                
            }
            
            sessions.Add(new ProxySession(listener.LocalAddress, matchConfig, listener));
        }
        
        return sessions.ToArray();
    }

    public override async Task<ProxyClient> CreateClientAsync(CreateRequest request, CancellationToken ctk = default)
    {
        var (config, json) = GenConfig(request.Method);
        var directory = _configuration[ConfigNames.ShadowSocks.ConfigsRoot]!;
        
        RequireDirectory(directory);
        await ActivateAsync(await SaveConfigAsync(directory, json, ctk)).ConfigureAwait(false);
        
        return new ProxyClient(request.Name, config, json, EncodeInline(config));
    }
    
    private ConfigTuple GenConfig(string method)
    {
        // TODO: IConfigGenerator.Create
        
        var config = new SocksConfig(
            _configuration[ConfigNames.ServerIp]!,
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

        var files = await _configProvider.GetFilesAsync(ctk);
        var filename = _configProvider.NamingStrategy.GetNewName(SocksConfig.Extension, files);
        
        var savePath = Path.Combine(dir, filename);
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

    private readonly record struct ConfigTuple(SocksConfig Config, string ConfigJson);
}