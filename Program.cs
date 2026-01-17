using System.ComponentModel;
using Spectre.Console.Cli;

Console.WriteLine();
var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<MakeModel>("make:model")
          .WithDescription("Makes a new database model.")
          .WithExample(new[] { "make:model", "Person" });
});

app.Run(args);