using RmPm.Core.Services.Socks;
using Serilog.Core;

namespace RmPm.Commands;

public class DeleteClientCommand : Command
{
    private readonly SocksManager _pm;
    private readonly Logger _logger;
    private readonly InputHelper _inputHelper;
    private readonly string _argument;

    public DeleteClientCommand(SocksManager pm, Logger logger, InputHelper inputHelper, string argument)
    {
        _pm = pm;
        _logger = logger;
        _inputHelper = inputHelper;
        _argument = argument;
    }
    
    public override async Task ExecuteAsync()
    {
        var config = await _inputHelper.FindConfigAsync(_argument);

        if (config is not null)
        {
            await _pm.DeleteClientAsync(config);
        }
        else
        {
            _logger.Warning("The client config not found");
        }
    }
}