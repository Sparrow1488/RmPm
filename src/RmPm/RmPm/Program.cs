﻿using System.Reflection;
using Microsoft.Extensions.Configuration;
using RmPm;
using RmPm.Commands;
using RmPm.Core.Services;
using RmPm.Core.Services.Socks;
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
    { "get", new ReadConfigCommand(configReader, inputHelper, logger, findConfigArgument, readConfigFormat) },
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