using RmPm.Core.Configuration;
using RmPm.Core.Services;
using RmPm.Core.Services.Socks;
using Serilog.Core;

namespace RmPm.Commands;

public class DeleteClientCommand : Command
{
    private readonly SocksManager _pm;
    private readonly Store _store;
    private readonly Logger _logger;
    private readonly string _argument;

    public DeleteClientCommand(SocksManager pm, Store store, Logger logger, string argument)
    {
        _pm = pm;
        _store = store;
        _logger = logger;
        _argument = argument;
    }
    
    public override async Task ExecuteAsync()
    {
        if (string.IsNullOrWhiteSpace(_argument))
            throw new Exception("No delete argument");
        
        var sessions = await _pm.GetSessionsAsync();

        ProxyClientConfig? config;

        if (int.TryParse(_argument, out _)) // PID
        {
            config = sessions.FirstOrDefault(x => x.Listener.Pid == _argument)?.Config;
            _logger.Information("Found config by PID");
        }
        else if (Guid.TryParse(_argument, out var id)) // ID
        {
            var entry = (await _store.GetAllAsync()).FirstOrDefault(x => x.Id == id);
            config = FindBy(sessions, entry?.ConfigPath ?? "-");
            
            _logger.Information("Found config by ID");
        }
        else // FriendlyName
        {
            var entry = (await _store.GetAllAsync()).FirstOrDefault(x => x.FriendlyName == _argument);
            config = FindBy(sessions, entry?.ConfigPath ?? "-");
            
            _logger.Information("Found config by FriendlyName");
        }

        if (config is SocksConfig socksConfig)
        {
            await _pm.DeleteClientAsync(socksConfig);
        }
        else
        {
            _logger.Warning("The client is not active or is not a ShadowSocks client");
        }
    }

    private static SocksConfig? FindBy(ProxySession[] sessions, string configPath)
    {
        return (SocksConfig?) sessions.FirstOrDefault(x => (x.Config as SocksConfig)?.FilePath == configPath)?.Config;
    }
}