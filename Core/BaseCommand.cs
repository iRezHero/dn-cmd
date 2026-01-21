using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

internal abstract class BaseCommand<TSettings> : Command<TSettings>
    where TSettings : CommandSettings
{
    /// <summary>
    /// Logs an informational message to the console.
    /// </summary>
    /// <param name="message"></param>
    protected void Info(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }

    /// <summary>
    /// Logs a warning message to the console.
    /// </summary>
    /// <param name="message"></param>
    protected void Warn(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{message}[/]");
    }

    /// <summary>
    /// Logs a debug message to the console.
    /// </summary>
    /// <param name="message"></param>
    protected void Debug(string message)
    {
        AnsiConsole.MarkupLine($"[grey]{message}[/]");
    }

    /// <summary>
    /// Logs an error message to the console.
    /// </summary>
    /// <param name="message"></param>
    protected void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }

    /// <summary>
    /// Processes a collection with a progress bar.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="action"></param>
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
