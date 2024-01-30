namespace RmPm.Core;

public record ProxyClient(
    string FriendlyName, 
    ClientConfig Config, 
    string ConfigString,
    string ConfigBase64
);
