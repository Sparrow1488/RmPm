using RmPm.Core.Models;
using RmPm.Core.Services;

namespace RmPm.Core.Configuration;

public record ProxySession(
    string Address,
    ProxyClientConfig? Config,
    EntryStore? Entry,
    NetListener Listener
);