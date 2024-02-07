using Newtonsoft.Json;

namespace RmPm.Core.Configuration;

/// <summary>
/// Конфигурация прокси-клиента
/// </summary>
public abstract record ProxyClientConfig;

public record SocksConfig(
    [JsonProperty("server")] string Server,
    [JsonProperty("server_port")] int ServerPort,
    [JsonProperty("local_port")] int LocalPort,
    [JsonProperty("password")] string Password,
    [JsonProperty("method")] string Method
) : ProxyClientConfig
{
    public const string Extension = ".json";
    [JsonIgnore]
    public string? FilePath { get; internal set; }
    [JsonIgnore]
    public string? FriendlyName { get; internal set; }
}