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

    var pm = new ProcessManager(logger);
    var socks = new ShadowSocksManager(configuration, pm, logger);

    // await CreateClientAsync(socks, clientName, logger);
    await ShowActiveSessionsAsync(socks);
}
catch (Exception ex)
{
    logger.Error(ex, ex.Message);
    throw;
}

static async Task CreateClientAsync(ProxyManager proxyManager, string clientName, ILogger logger)
{
    var client = await proxyManager.CreateClientAsync(new CreateRequest(clientName, Methods.ChaCha));
    
    logger.Information("{client} created success", clientName);
    Console.WriteLine(client.ConfigString);
    Console.WriteLine(client.ConfigBase64);
}

static async Task ShowActiveSessionsAsync(ShadowSocksManager ssManager)
{
    var sessions = await ssManager.GetSessionsAsync();

    foreach (var session in sessions)
        Console.WriteLine(session.Address + " config: " + session.Config);
}