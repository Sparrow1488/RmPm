using System.Reflection;
using Microsoft.Extensions.Configuration;
using RmPm;
using RmPm.Commands;
using RmPm.Core.Services.Auxiliary;
using RmPm.Core.Services.Managers;
using RmPm.Core.Services.Socks;
using RmPm.Core.Services.Storage;
using Serilog;

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
    .WriteTo.Console(outputTemplate: "{Level:u3}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

#endregion

#region Initialize

var pm = new ProcessManager();
var jsonService = new JsonService();
var configReader = new SocksConfigReader(jsonService);
var store = new Store(new LocalStore(AppContext.BaseDirectory), jsonService, logger);
// NOTE: файл без номера это конфигурация ShadowSocks, а не клиентов
var configs = new SocksConfigProvider(configuration, configReader, store, logger, file => file.Number is not null);
var socks = new SocksManager(configs, pm, store);
var inputHelper = new InputHelper(store, socks, logger);

await store.RestoreAsync(await configs.GetAllAsync());

#endregion

#region CLI Commands

var creationClientName = args.Length > 1 ? args[1] : null;
var findConfigArgument = args.Length > 1 ? args[1] : "";
var readConfigFormat = args.Length > 2 ? args[2] : "";

var commands = new Dictionary<string, Command>
{
    { "new", new CreateClientCommand(socks, configReader, logger, creationClientName) },
    { "all", new GetSessionsCommand(socks, logger) },
    { "del", new DeleteClientCommand(socks, logger, inputHelper, findConfigArgument) },
    { "get", new ReadConfigCommand(configReader, inputHelper, findConfigArgument, readConfigFormat) },
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