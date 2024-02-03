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
    var pm = new ProcessManager(logger);
    var socks = new ShadowSocksManager(configuration, pm, logger);

    // await CreateClientAsync(socks, clientName, logger);
    await ShowActiveSessionsAsync(socks, logger);
}
catch (Exception ex)
{
    logger.Error(ex, ex.Message);
    throw;
}

static async Task CreateClientAsync(ProxyManager proxyManager, ILogger logger)
{
    const string clientName = "Sparrow";
    
    logger.Information("Creating client {client}", clientName);
    
    var client = await proxyManager.CreateClientAsync(new CreateRequest(clientName, Methods.ChaCha));
    
    logger.Information("{client} created success", clientName);
    Console.WriteLine(client.ConfigString);
    Console.WriteLine(client.ConfigBase64);
}

static async Task ShowActiveSessionsAsync(ShadowSocksManager ssManager, ILogger logger)
{
    logger.Information("Get active proxy sessions");
    
    var sessions = await ssManager.GetSessionsAsync();

    foreach (var session in sessions)
        logger.Information(
            "[PID:{pid}] Listen {address}" + session.Config, 
            session.Listener.Pid, 
            session.Address
        );
}