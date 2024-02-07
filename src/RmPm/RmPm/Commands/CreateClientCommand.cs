using RmPm.Core.Configuration;
using RmPm.Core.Models;
using RmPm.Core.Services;
using RmPm.Core.Services.Socks;
using Serilog;

namespace RmPm.Commands;

public class CreateClientCommand : Command
{
    private readonly ProxyManager _pm;
    private readonly SocksConfigReader _configReader;
    private readonly ILogger _logger;
    private readonly string? _clientName;

    public CreateClientCommand(ProxyManager pm, SocksConfigReader configReader, ILogger logger, string? clientName)
    {
        _pm = pm;
        _configReader = configReader;
        _logger = logger;
        _clientName = clientName;
    }
    
    public override async Task ExecuteAsync()
    {
        const string clientName = "Sparrow";
    
        _logger.Information("Creating client {client}", clientName);
    
        var config = await _pm.CreateClientAsync(new CreateClientRequest(_clientName));
    
        _logger.Information("{client} created success", clientName);
        Console.WriteLine(config);
        Console.WriteLine(_configReader.ToBase64((SocksConfig) config));
    }
}