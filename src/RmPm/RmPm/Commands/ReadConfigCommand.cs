using RmPm.Core.Configuration;
using RmPm.Core.Contracts;
using Serilog;

namespace RmPm.Commands;

public class ReadConfigCommand : Command
{
    private readonly InputHelper _inputHelper;
    private readonly ILogger _logger;
    private readonly string _findArgument;
    private readonly string _format;
    private readonly Dictionary<string, Func<SocksConfig, object>> _formatDict;

    public ReadConfigCommand(
        IConfigReader<SocksConfig> configReader, 
        InputHelper inputHelper, 
        ILogger logger,
        string findArgument, 
        string format
    )
    {
        _inputHelper = inputHelper;
        _logger = logger;
        _findArgument = findArgument;
        _format = format;

        _formatDict = new Dictionary<string, Func<SocksConfig, object>>
        {
            { "json", configReader.ToJson },
            { "base64", c => configReader.ToBase64(c, tag: "RmPmClient") },
            { "qr", c => throw new NotImplementedException() }
        };
    }
    
    public override async Task ExecuteAsync()
    {
        if (string.IsNullOrWhiteSpace(_format))
            throw new Exception("Invalid input format");

        var config = await _inputHelper.FindConfigAsync(_findArgument);

        if (config is not null)
        {
            var formatted = _formatDict[_format].Invoke(config);
            Console.WriteLine(formatted);
        }
        else
        {
            _logger.Warning("The client config not found");
        }
    }
}