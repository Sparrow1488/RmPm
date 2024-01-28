using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RmPm.Core.Services;

public class ShadowSocksManager : ProxyManager
{
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerSettings _serializeSettings;

    public ShadowSocksManager(IConfiguration configuration)
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
    
    public override async Task<ProxyClient> CreateClientAsync(string name, CancellationToken ctk = default)
    {
        var config = GenConfig("chacha20-ietf-poly1305");
        var configJson = JsonConvert.SerializeObject(config, _serializeSettings);

        var saveDir = _configuration["Proxies:ShadowSocks:ConfigsDir"]!;
        if (!Directory.Exists(saveDir))
            throw new DirectoryNotFoundException(saveDir);

        var savePath = Path.Combine(saveDir, GetConfigName());
        await File.WriteAllTextAsync(savePath, configJson, ctk).ConfigureAwait(false);
        Console.WriteLine($"File wrote success:{File.Exists(savePath)}: " + savePath);

        await ActivateAsync().ConfigureAwait(false);

        Console.WriteLine(EncodeInline(config));
        
        return new ProxyClient(name, config, configJson);
    }

    private SocksConfig GenConfig(string method) => new(
        _configuration["Server:Ip"]!,
        Random.Shared.Next(2000, 9000),
        Random.Shared.Next(2000, 9000),
        Guid.NewGuid().ToString().Replace("-", ""),
        method
    );

    private async Task ActivateAsync()
    {
        var command = string.Join(" & ", GetAllConfigFiles().Select(x => "ss-server -c " + x));
        Console.WriteLine(command);

        try
        {
            using var src = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await BashAsync(command, src.Token).ConfigureAwait(false); // Утилиты ss-server, ss-local - х**ня, я считаю, что их авторы долджны гореть в аду за их кусок го*на
        } catch
        {
            // ignored
        }
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
        
        Console.WriteLine("Last config number is " + configLastNumber);

        return "config" + (configLastNumber == 0 ? 1 : ++configLastNumber) + ".json";
    }

    private string[] GetAllConfigFiles()
    {
        return Directory.GetFiles(_configuration["Proxies:ShadowSocks:ConfigsDir"]!)
            .Where(x => Path.GetFileNameWithoutExtension(x).Any(char.IsDigit))
            .ToArray();
    }
}