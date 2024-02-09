using System.CommandLine;

var root = new RootCommand("Tester.Cli App");

var nameOption = new Option<string>(new[] { "-n", "--name" }, "Ваше имя")
{
    Arity = ArgumentArity.ExactlyOne
};

var filesOption = new Option<string[]>(new[] { "-f", "--file" }, "Файлы для чтения")
{
    Arity = ArgumentArity.OneOrMore,
    IsRequired = true,
    AllowMultipleArgumentsPerToken = true
};

var languageOption = new Option<string>(new[] { "-l", "--language" }, "Язык программирования")
{
    Arity = ArgumentArity.ExactlyOne,
    IsRequired = true
}.FromAmong("csharp", "java", "kotlin");

var helloCommand = new Command("hello", "Поздороваться");
helloCommand.AddOption(nameOption);
helloCommand.SetHandler(
    x => Console.WriteLine($"Hello, {x}!"),
    nameOption
);

var readFilesCommand = new Command("read", "Прочитать файлы");
readFilesCommand.AddOption(filesOption);
readFilesCommand.AddOption(languageOption);
readFilesCommand.SetHandler(
    (files, language) => Console.WriteLine(string.Join(", ", files) + $"; language: {language}"),
    filesOption,
    languageOption
);

root.AddCommand(helloCommand);
root.AddCommand(readFilesCommand);

root.Invoke(args);