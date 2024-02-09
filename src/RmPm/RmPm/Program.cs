using System.CommandLine;
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

var root = new RootCommand();

var clientNameOption = new Option<string>(new[] {"-n", "--name"}, "Имя клиента")
{
    IsRequired = true,
    Arity = ArgumentArity.ExactlyOne
};

var findClientOption = new Option<string>(new[] {"-c", "--client"}, "Идентификатор пользователя (PID, ID, Name)")
{
    IsRequired = true,
    Arity = ArgumentArity.ExactlyOne
};

var configFormatOption = new Option<string>(new[] {"-f", "--format"}, "Формат чтения файла настроек")
{
    IsRequired = true,
    Arity = ArgumentArity.ExactlyOne
}.FromAmong("json", "base64");

var newCommand = new Command("new", "Создать нового клиента");
newCommand.AddOption(clientNameOption);
newCommand.SetHandler(
    client => new CreateClientCommand(socks, configReader, logger, client).ExecuteAsync(), 
    clientNameOption
);

var getSessionsCommand = new Command("all", "Показать всех активных клиентов");
getSessionsCommand.SetHandler(() => new GetSessionsCommand(socks, logger).ExecuteAsync());

var deleteCommand = new Command("del", "Удалить пользователя");
deleteCommand.AddOption(findClientOption);
deleteCommand.SetHandler(
    findClient => new DeleteClientCommand(socks, logger, inputHelper, findClient).ExecuteAsync(), 
    findClientOption
);

var readConfigCommand = new Command("read", "Прочитать файл настроек клиента в указанном формате");
readConfigCommand.AddOption(findClientOption);
readConfigCommand.AddOption(configFormatOption);
readConfigCommand.SetHandler(
    (findClient, format) => new ReadConfigCommand(configReader, inputHelper, findClient, format).ExecuteAsync(), 
    findClientOption, 
    configFormatOption
);

root.AddCommand(newCommand);
root.AddCommand(getSessionsCommand);
root.AddCommand(deleteCommand);
root.AddCommand(readConfigCommand);

#endregion

try
{
    await root.InvokeAsync(args);
}
catch (Exception ex)
{
    logger.Error(ex, ex.Message);
    throw;
}