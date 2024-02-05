using System.Reflection;
using Microsoft.Extensions.Configuration;
using RmPm.Core;
using RmPm.Core.Services;
using RmPm.Core.Services.Socks;
using Serilog;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json") // TODO: provide from args
#if DEBUG
    .AddUserSecrets(Assembly.GetAssembly(typeof(Program))!)
#endif
    .Build();

var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

#if DEBUG
    logger.Debug("Mode=DEBUG");
#else
    logger.Debug("Mode=RELEASE");
#endif

logger.Information("RmPm started");

try
{
    var pm = new ProcessManager(logger);
    var jsonService = new JsonService();
    // NOTE: файл без номера это конфигурация ShadowSocks, а не клиентов
    var configs = new SocksConfigProvider(configuration, jsonService, logger, file => file.Number is not null);
    var socks = new SocksManager(configs, pm, configuration, logger);

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
    
    var client = await proxyManager.CreateClientAsync(new CreateRequest(clientName, Encrypt.ShadowSocks.ChaCha20));
    
    logger.Information("{client} created success", clientName);
    Console.WriteLine(client.ConfigString);
    Console.WriteLine(client.ConfigBase64);
}

static async Task ShowActiveSessionsAsync(SocksManager ssManager, ILogger logger)
{
    logger.Information("Get active proxy sessions");
    
    var sessions = await ssManager.GetSessionsAsync();

    foreach (var session in sessions)
    {
        logger.Information(
            "[PID:{pid}] Listen {address}",
            session.Listener.Pid, 
            session.Address
        );
        
        Console.WriteLine(session.Config); // TODO: config path
    }
}