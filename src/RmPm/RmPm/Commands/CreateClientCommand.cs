using RmPm.Core.Configuration;
using RmPm.Core.Models;
using RmPm.Core.Services.Managers;
using RmPm.Core.Services.Socks;
using Serilog;

namespace RmPm.Commands;

public class CreateClientCommand : MyCommand
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
        var config = (SocksConfig) await _pm.CreateClientAsync(new CreateClientRequest(_clientName));
    
        _logger.Information("{client} created", config.FriendlyName);
        
        Console.WriteLine("\n" + _configReader.ToJson(config));
        Console.WriteLine("\n" + _configReader.ToBase64(config));
    }
}