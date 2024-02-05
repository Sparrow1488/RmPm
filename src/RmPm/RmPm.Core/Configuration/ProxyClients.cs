using RmPm.Core.Services;

namespace RmPm.Core.Configuration;

public record ProxyClient(
    string FriendlyName, 
    ProxyClientConfig Config, 
    string ConfigString,
    string ConfigBase64
);

public record ProxySession(
    string Address,
    ProxyClientConfig? Config,
    NetListener Listener
);