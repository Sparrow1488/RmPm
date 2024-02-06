using System.Reflection;
using Microsoft.Extensions.Configuration;
using RmPm;
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

var pm = new ProcessManager(logger);
var jsonService = new JsonService();
// NOTE: файл без номера это конфигурация ShadowSocks, а не клиентов
var configs = new SocksConfigProvider(configuration, jsonService, logger, file => file.Number is not null);
var socks = new SocksManager(configs, pm, logger);

var commands = new Dictionary<string, Command>
{
    { "create", new CreateClientCommand(socks, logger) },
    { "sessions", new GetSessionsCommand(socks, logger) },
    { "", new GetSessionsCommand(socks, logger) }
};

try
{
    var executionsArg = args.FirstOrDefault() ?? string.Empty;
    await commands[executionsArg].ExecuteAsync();
}
catch (Exception ex)
{
    logger.Error(ex, ex.Message);
    throw;
}