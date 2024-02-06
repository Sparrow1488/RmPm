using System.Reflection;
using Microsoft.Extensions.Configuration;
using RmPm;
using RmPm.Core.Configuration;
using RmPm.Core.Contracts;
using RmPm.Core.Models;
using RmPm.Core.Services;
using RmPm.Core.Services.Socks;
using Serilog;
using Serilog.Core;

#region Configuration

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json") // TODO: provide from args
#if DEBUG
    .AddUserSecrets(Assembly.GetAssembly(typeof(Program))!)
#endif
    .Build();

#endregion

#region Logger

var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

#endregion

#region Write Mode

#if DEBUG
logger.Debug("Mode=DEBUG");
#else
    logger.Debug("Mode=RELEASE");
#endif

#endregion

logger.Information("RmPm started");

#region Initialize

var pm = new ProcessManager(logger);
var jsonService = new JsonService();
var store = new Store(new LocalStore(AppContext.BaseDirectory), jsonService, logger);
// NOTE: файл без номера это конфигурация ShadowSocks, а не клиентов
var configs = new SocksConfigProvider(configuration, jsonService, store, logger, file => file.Number is not null);
var socks = new SocksManager(configs, pm, logger);

await store.RestoreAsync(await configs.GetAllAsync());

#endregion

#region CLI Commands

var commands = new Dictionary<string, Command>
{
    { "create", new CreateClientCommand(socks, logger) },
    { "sessions", new GetSessionsCommand(socks, logger) },
    { "delete", new DeleteClientCommand(socks, logger, args.Length > 1 ? args[1] : "-1") }, // NOTE: да, пока что удаление будет по PID ору
    { "", new GetSessionsCommand(socks, logger) }
};

#endregion

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