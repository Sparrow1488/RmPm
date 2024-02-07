using RmPm.Core.Services.Socks;
using Serilog;

namespace RmPm.Commands;

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