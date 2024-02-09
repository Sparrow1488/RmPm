using RmPm.Core.Models;
using RmPm.Core.Models.Storage;
using RmPm.Core.Services.Utilities;

namespace RmPm.Core.Configuration;

public record ProxySession(
    string Address,
    ProxyClientConfig? Config,
    EntryStore? Entry,
    NetListener Listener
);