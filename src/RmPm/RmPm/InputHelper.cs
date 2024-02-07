using RmPm.Core.Configuration;
using RmPm.Core.Services;
using RmPm.Core.Services.Socks;
using Serilog;

namespace RmPm;

public class InputHelper
{
    private readonly Store _store;
    private readonly SocksManager _socksManager;
    private readonly ILogger _logger;

    public InputHelper(Store store, SocksManager socksManager, ILogger logger)
    {
        _store = store;
        _socksManager = socksManager;
        _logger = logger;
    }
    
    public async Task<SocksConfig?> FindConfigAsync(string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new Exception("No delete argument");
        
        var sessions = await _socksManager.GetSessionsAsync();

        ProxyClientConfig? config;

        if (int.TryParse(argument, out _)) // PID
        {
            config = sessions.FirstOrDefault(x => x.Listener.Pid == argument)?.Config;
            _logger.Information("Found config by PID");
        }
        else if (Guid.TryParse(argument, out var id)) // ID
        {
            var entry = (await _store.GetAllAsync()).FirstOrDefault(x => x.Id == id);
            config = FindBy(sessions, entry?.ConfigPath ?? "-");
            
            _logger.Information("Found config by ID");
        }
        else // FriendlyName
        {
            var entry = (await _store.GetAllAsync()).FirstOrDefault(x => x.FriendlyName == argument);
            config = FindBy(sessions, entry?.ConfigPath ?? "-");
            
            _logger.Information("Found config by FriendlyName");
        }

        return (SocksConfig?) config;
    }

    private static SocksConfig? FindBy(ProxySession[] sessions, string configPath)
    {
        return (SocksConfig?) sessions.FirstOrDefault(x => (x.Config as SocksConfig)?.FilePath == configPath)?.Config;
    }
}