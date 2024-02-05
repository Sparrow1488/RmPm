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
}