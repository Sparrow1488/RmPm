using Microsoft.Extensions.Configuration;
using RmPm.Core;
using RmPm.Core.Services;
using Serilog;

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

logger.Information("RmPm started");

try
{
    const string clientName = "Sparrow";
    logger.Information("Creating client {client}", clientName);
    
    var socks = new ShadowSocksManager(configuration, logger);
    var client = await socks.CreateClientAsync(new CreateRequest(clientName, Methods.ChaCha));
    
    logger.Information("{client} created success", clientName);
    Console.WriteLine(client.ConfigString);
    Console.WriteLine(client.ConfigBase64);
}
catch (Exception ex)
{
    logger.Error(ex, ex.Message);
    throw;
}