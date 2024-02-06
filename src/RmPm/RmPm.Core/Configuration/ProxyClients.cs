using RmPm.Core.Services;

namespace RmPm.Core.Configuration;

public record ProxyClient(
    ProxyClientConfig Config, 
    string ConfigBase64
);

public record ProxySession(
    string Address,
    ProxyClientConfig? Config,
    NetListener Listener
);
