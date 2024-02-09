// See https://aka.ms/new-console-template for more information

using RmPm.Core.Services.Managers;

var file = @"C:\Users\ilyao\OneDrive\Desktop\Development\Projects\RmPm\src\RmPm\Tester.Loop\Loop\Tester.Loop.exe";

var pm = new ProcessManager();
var run = pm.RunAsync(new RunArgs(file, "", false, TimeSpan.FromSeconds(1.2)));

await Task.Delay(2500);

await run;