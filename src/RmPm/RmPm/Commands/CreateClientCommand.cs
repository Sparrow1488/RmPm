using RmPm.Core.Services;
using Serilog;

namespace RmPm.Commands;

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