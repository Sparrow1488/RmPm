using RmPm.Core.Configuration;
using RmPm.Core.Services.Socks;
using Serilog;

namespace RmPm.Commands;

public class GetSessionsCommand : MyCommand
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
        var sessions = await _pm.GetSessionsAsync();
        
        if (sessions.Length == 0)
            _logger.Information("No clients are running");
        
        foreach (var session in sessions)
            ShowSession(session);
    }

    private void ShowSession(ProxySession session)
    {
        var config = (SocksConfig?) session.Config;
        var entry = session.Entry;

        if (entry is null || config is null)
        {
            _logger.Warning("Entry or config not found to display {address}", session.Address);
            return;
        }
        
        _logger.Information(
            "[{id}][{pid}][{name}] Listen {address}, '{path}'",
            entry.Id,
            session.Listener.Pid,
            entry.FriendlyName,
            session.Address,
            config.FilePath
        );
    }
}