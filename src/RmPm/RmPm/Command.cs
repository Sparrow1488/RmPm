using RmPm.Core.Configuration;
using RmPm.Core.Models;
using RmPm.Core.Services;
using RmPm.Core.Services.Socks;
using Serilog;
using Serilog.Core;

namespace RmPm;

public abstract class Command
{
    public abstract Task ExecuteAsync();
}

public class CreateClientCommand : Command
{
    private readonly ProxyManager _pm;
    private readonly ILogger _logger;

    public CreateClientCommand(ProxyManager pm, ILogger logger)
    {
        _pm = pm;
        _logger = logger;
    }
    
    public override async Task ExecuteAsync()
    {
        const string clientName = "Sparrow";
    
        _logger.Information("Creating client {client}", clientName);
    
        var client = await _pm.CreateClientAsync();
    
        _logger.Information("{client} created success", clientName);
        Console.WriteLine(client.Config);
        Console.WriteLine(client.ConfigBase64);
    }
}

public class GetSessionsCommand : Command
{
    private readonly SocksManager _pm;
    private readonly ILogger _logger;

    public GetSessionsCommand(SocksManager pm, ILogger logger)
    {
        _pm = pm;
        _logger = logger;
    }
    
    public override async Task ExecuteAsync()
    {
        _logger.Information("Get active proxy sessions");
    
        var sessions = await _pm.GetSessionsAsync();

        foreach (var session in sessions)
        {
            _logger.Information(
                "[PID:{pid}] Listen {address}",
                session.Listener.Pid, 
                session.Address
            );
        
            Console.WriteLine(session.Config); // TODO: config path
        }
    }
}

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