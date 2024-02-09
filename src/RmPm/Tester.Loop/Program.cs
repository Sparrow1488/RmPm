// See https://aka.ms/new-console-template for more information

using RmPm.Core.Services.Managers;
using RmPm.Core.Services.Managers.Arguments;

var src = new CancellationTokenSource();

var file = @"C:\Users\ilyao\OneDrive\Desktop\Development\Projects\RmPm\src\RmPm\Tester.Loop\Loop\Tester.Loop.exe";

var pm = new ProcessManager();
var run = pm.RunAsync(new RunArgs(file, "", true, TimeSpan.FromSeconds(2.5)), src.Token);

// await Task.Delay(1000);
//     
// Console.WriteLine("Dispose");
// src.Cancel();
// src.Dispose();

await Task.Delay(3000);

Console.WriteLine(await run);