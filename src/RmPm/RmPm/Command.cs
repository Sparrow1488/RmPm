using RmPm.Core.Configuration;
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
    private readonly Logger _logger;
    private readonly string _pid;

    public DeleteClientCommand(SocksManager pm, Logger logger, string pid)
    {
        _pm = pm;
        _logger = logger;
        _pid = pid;
    }
    
    public override async Task ExecuteAsync()
    {
        var sessions = await _pm.GetSessionsAsync();
        var clientSession = sessions.FirstOrDefault(x => x.Listener.Pid == _pid);

        if (clientSession?.Config is SocksConfig config)
        {
            await _pm.DeleteClientAsync(config);
        }
        else
        {
            _logger.Warning("The client is not active or is not a ShadowSocks client");
        }
    }
}