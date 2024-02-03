namespace RmPm.Core;

public record ProxyClient(
    string FriendlyName, 
    ClientConfig Config, 
    string ConfigString,
    string ConfigBase64
);

public record ProxySession(
    string Address,
    ClientConfig? Config
);
