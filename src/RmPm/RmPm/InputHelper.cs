using RmPm.Core.Configuration;
using RmPm.Core.Models;
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
    
    /// <summary>
    /// Позволяет найти конфиг файл клиента по ID или FriendlyName из записи, а также по PID процесса
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<SocksConfig?> FindConfigAsync(string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("Search argument is null or empty", nameof(argument));
        
        var sessions = await _socksManager.GetSessionsAsync();

        ProxyClientConfig? config;
        string searchBy;

        if (int.TryParse(argument, out _))
        {
            config = sessions.FirstOrDefault(x => x.Listener.Pid == argument)?.Config;
            searchBy = "PID";
        }
        else if (Guid.TryParse(argument, out var id))
        {
            config = await FindAsync(sessions, e => e.Id == id);
            searchBy = "ID";
        }
        else
        {
            config = await FindAsync(sessions, e => e.FriendlyName == argument);
            searchBy = "FriendlyName";
        }
        
        _logger.Information("Search config by {by}", searchBy);

        return (SocksConfig?) config;
    }

    private async Task<SocksConfig?> FindAsync(ProxySession[] sessions, Func<EntryStore, bool> search)
    {
        var entry = (await _store.GetAllAsync()).FirstOrDefault(search.Invoke);
        return FindBy(sessions, entry?.ConfigPath ?? "-");
    }

    private static SocksConfig? FindBy(ProxySession[] sessions, string configPath)
    {
        return (SocksConfig?) sessions.FirstOrDefault(x => (x.Config as SocksConfig)?.FilePath == configPath)?.Config;
    }
}