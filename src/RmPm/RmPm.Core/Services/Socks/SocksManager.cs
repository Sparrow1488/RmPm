using System.Text;
using RmPm.Core.Configuration;
using RmPm.Core.Contracts;
using RmPm.Core.Extensions;
using RmPm.Core.Models;
using Serilog;
using Serilog.Data;

namespace RmPm.Core.Services.Socks;

public class SocksManager : ProxyManager
{
    private const string LogContext = "ss-manager";

    private readonly IConfigFileProvider<SocksConfig> _configProvider;
    private readonly NetStat _netStats;

    public SocksManager(
        IConfigFileProvider<SocksConfig> configProvider,
        IProcessManager pm,
        ILogger logger
    ) : base(pm, logger)
    {
        _configProvider = configProvider;
        _netStats = new NetStat(pm);
    }

    public async Task<ProxySession[]> GetSessionsAsync(CancellationToken ctk = default)
    {
        var configs = await _configProvider.GetAllAsync(ctk);
        
        var items = await _netStats.GetListenersAsync();
        var ssListeners = items.Where(x => x.ProgramName.StartsWith("ss-")).ToArray();
        
        Logger.Debug("[{ctx}] Found {count} ss listeners", LogContext, ssListeners.Length);

        var sessions = new List<ProxySession>();

        foreach (var listener in ssListeners)
        {
            var address = IPv4Address.Parse(listener.LocalAddress);
            
            if (address is null)
            {
                Logger.Debug("[{ctx}] Invalid listener ({pid}) address {value}", LogContext, listener.Pid, listener.LocalAddress);
                continue;
            }
            
            var matchConfig = configs.FirstOrDefault(c => c.ServerPort == address.Value.Port);

            if (matchConfig == default)
            {
                Logger.Debug("[{ctx}] Config for listener {address} no match", LogContext, listener.LocalAddress);
                continue;                
            }
            
            sessions.Add(new ProxySession(listener.LocalAddress, matchConfig, listener));
        }
        
        return sessions.ToArray();
    }

    public override async Task<ProxyClient> CreateClientAsync(CancellationToken ctk = default)
    {
        var config = await _configProvider.GenerateAsync(ctk);
        var path = await _configProvider.SaveAsync(config, ctk);
        
        var command = $"ss-server -c {path} & ";
        await BashAsync(command, timeout: TimeSpan.FromSeconds(1));
        
        return new ProxyClient(config, EncodeInline(config));
    }

    private static string EncodeInline(SocksConfig config)
    {
        var line = config.Method + ":" + config.Password + "@" + config.Server + ":" + config.ServerPort;
        return "ss://" + Convert.ToBase64String(Encoding.UTF8.GetBytes(line)) + "\n";
    }

    public async Task DeleteClientAsync(SocksConfig config, CancellationToken ctk = default)
    {
        var sessions = await GetSessionsAsync(ctk);
        var session = sessions.FirstOrDefault(x => x.Config == config);

        if (session is not null)
        {
            await ProcessManager.KillAsync(session.Listener.Pid, ctk);
            Logger.Debug("Kill client {client}", session.Listener.Pid);
        }
        else
        {
            var address = $"{config.Server}:{config.ServerPort}";
            Logger.Debug("Client is not active {client}", address);
        }

        await _configProvider.DeleteAsync(config, ctk);
        Logger.Debug("Delete client config");
    }
}