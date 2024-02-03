using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RmPm.Core.Contracts;
using Serilog;

namespace RmPm.Core.Services;

public class ShadowSocksManager : ProxyManager
{
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerSettings _serializeSettings;

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
    }
    
    public override async Task<ProxyClient> CreateClientAsync(CreateRequest request, CancellationToken ctk = default)
    {
        var (config, json) = GenConfig(request.Method);
        var directory = _configuration["Proxies:ShadowSocks:ConfigsDir"]!;
        
        RequireDirectory(directory);
        await ActivateAsync(await SaveConfigAsync(directory, json, ctk), ctk).ConfigureAwait(false);
        
        return new ProxyClient(request.Name, config, json, EncodeInline(config));
    }

    private ConfigTuple GenConfig(string method)
    {
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
        var savePath = Path.Combine(dir, GetConfigName());
        await File.WriteAllTextAsync(savePath, config, ctk).ConfigureAwait(false);
        Logger.Debug("Config saved {status}, path '{path}'", File.Exists(savePath) ? "success" : "failed", savePath);

        return savePath;
    }

    private async Task ActivateAsync(string configPath, CancellationToken ctk = default)
    {
        var command = $"ss-server -c {configPath} & ";
        await BashAsync(command, timeout: TimeSpan.FromSeconds(2), ctk);
    }

    private static string EncodeInline(SocksConfig config)
    {
        var line = config.Method + ":" + config.Password + "@" + config.Server + ":" + config.ServerPort;
        return "ss://" + Convert.ToBase64String(Encoding.UTF8.GetBytes(line)) + "\n";
    }

    private string GetConfigName()
    {
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
        return Directory.GetFiles(_configuration["Proxies:ShadowSocks:ConfigsDir"]!)
            .Where(x => Path.GetFileNameWithoutExtension(x).Any(char.IsDigit))
            .ToArray();
    }

    private readonly record struct ConfigTuple(SocksConfig Config, string ConfigJson);
}