using RmPm.Core.Configuration;
using RmPm.Core.Contracts;
using RmPm.Core.Extensions;
using RmPm.Core.Models;
using RmPm.Core.Services.Managers;
using RmPm.Core.Services.Storage;
using RmPm.Core.Services.Utilities;

namespace RmPm.Core.Services.Socks;

public class SocksManager : ProxyManager
{
    private readonly IConfigFileProvider<SocksConfig> _configProvider;
    private readonly Store _store;
    private readonly NetStat _netStats;

    public SocksManager(
        IConfigFileProvider<SocksConfig> configProvider,
        IProcessManager pm,
        Store store
    ) : base(pm)
    {
        _configProvider = configProvider;
        _store = store;
        _netStats = new NetStat(pm);
    }

    public async Task<ProxySession[]> GetSessionsAsync(CancellationToken ctk = default)
    {
        var configs = await _configProvider.GetAllAsync(ctk);
        var entries = await _store.GetAllAsync(ctk);
        
        var items = await _netStats.GetListenersAsync();
        var ssListeners = items.Where(x => x.ProgramName.StartsWith("ss-")).ToArray();
        
        var sessions = new List<ProxySession>();

        foreach (var listener in ssListeners)
        {
            var address = IPv4Address.Parse(listener.LocalAddress);
            
            if (address is null)
            {
                // Invalid listener address
                continue;
            }
            
            var config = configs.FirstOrDefault(c => c.ServerPort == address.Value.Port);
            var entry = entries.FirstOrDefault(x => x.ConfigPath == config?.FilePath);
            
            sessions.Add(new ProxySession(listener.LocalAddress, config, entry, listener));
        }
        
        return sessions.ToArray();
    }

    public override async Task<ProxyClientConfig> CreateClientAsync(CreateClientRequest request, CancellationToken ctk = default)
    {
        var config = await _configProvider.GenerateAsync(ctk);
        config.FriendlyName = request.FriendlyName;
        
        await BashAsync(new BashRunSocks(await _configProvider.SaveAsync(config, ctk)));
        return config;
    }

    public async Task DeleteClientAsync(SocksConfig config, CancellationToken ctk = default)
    {
        var sessions = await GetSessionsAsync(ctk);
        var session = sessions.FirstOrDefault(x => x.Config == config);

        if (session is not null)
        {
            await ProcessManager.KillAsync(session.Listener.Pid, ctk);
        }

        await _configProvider.DeleteAsync(config, ctk);
    }
}