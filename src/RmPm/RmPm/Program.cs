// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using RmPm.Core.Services;

Console.WriteLine("RmPm started");

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

try
{
    Console.WriteLine("Creating client");
    
    var socks = new ShadowSocksManager(configuration);
    var client = await socks.CreateClientAsync("sparrow");
    
    Console.WriteLine("Created success");
    Console.WriteLine(client.ConfigString);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    throw;
}
