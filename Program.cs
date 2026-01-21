using System.ComponentModel;
using System.Data;
using Spectre.Console.Cli;

Console.WriteLine();
var app = new CommandApp();

app.Configure(config =>
{
    config
        .AddCommand<MakeModel>("make:model")
        .WithDescription("Makes a new database model.")
        .WithExample(new[] { "make:model", "Person" });

    config
        .AddCommand<MakeCommand>("make:command")
        .WithDescription("Makes a new command.")
        .WithExample(new[] { "make:command", "MyCommand" });
});

app.Configure(config =>
{
    var consoleCommandsDirectory = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Console",
        "Commands"
    );
    if (Directory.Exists(consoleCommandsDirectory))
    {
        foreach (var file in Directory.GetFiles(consoleCommandsDirectory, "*.cs"))
        {
            var commandName = Path.GetFileNameWithoutExtension(file);
            var commandType = Type.GetType(commandName);
            if (commandType != null)
            {
                var method = config.GetType().GetMethod("AddCommand");
                var genericMethod = method!.MakeGenericMethod(commandType);
                genericMethod.Invoke(config, new object[] { commandName.ToLowerInvariant() });
            }
        }
    }
});

app.Run(args);
