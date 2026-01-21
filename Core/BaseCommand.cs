using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

internal abstract class BaseCommand<TSettings> : Command<TSettings>
    where TSettings : CommandSettings
{
    protected void Info(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }

    protected void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }

    protected void WithProgressBar<T>(IEnumerable<T> collection, Action<T> action)
    {
        var items = collection.ToList();
        if (!items.Any())
        {
            Info("No items to process.");
            return;
        }

        AnsiConsole
            .Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask("[green]Processing items...[/]", maxValue: items.Count);

                foreach (var item in items)
                {
                    action(item);
                    task.Increment(1);
                }
            });
    }
}
