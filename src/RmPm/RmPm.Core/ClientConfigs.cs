using Newtonsoft.Json;

namespace RmPm.Core;

public abstract record ClientConfig;

public record SocksConfig(
    [JsonProperty("server")] string Server,
    [JsonProperty("server_port")] int ServerPort,
    [JsonProperty("local_port")] int LocalPort,
    [JsonProperty("password")] string Password,
    [JsonProperty("method")] string Method
) : ClientConfig;