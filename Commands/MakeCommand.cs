using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Spectre.Console;
using Spectre.Console.Cli;

internal class MakeCommand : Command<MakeCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[name]")]
        [Description("The name of the command to create.")]
        public string Name { get; init; } = string.Empty;
    }

    public override int Execute(
        CommandContext context,
        Settings settings,
        CancellationToken cancellation
    )
    {
        var commandName = settings.Name;
        while (
            string.IsNullOrEmpty(commandName)
            || commandName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
        )
        {
            if (!string.IsNullOrEmpty(commandName))
            {
                AnsiConsole.MarkupLine(
                    "[red]ERROR:[/] Command name [yellow]'{0}'[/] contains invalid characters. Please try again.",
                    commandName
                );
            }
            commandName = AnsiConsole.Ask<string>("What the command should be called?");
        }

        // Normalize command name (PascalCase)
        commandName = NormalizeCommandName(commandName);

        var currentDirectory = Directory.GetCurrentDirectory();
        var commandsDirectory = Path.Combine(currentDirectory, "Console", "Commands");
        if (!Directory.Exists(commandsDirectory))
        {
            Directory.CreateDirectory(commandsDirectory);
        }
        var filePath = Path.Combine(currentDirectory, "Console", "Commands", $"{commandName}.cs");
        if (File.Exists(filePath))
        {
            AnsiConsole.MarkupLine(
                "[red]ERROR:[/] Command [yellow]'{0}'[/] already exists at '{1}'.",
                commandName,
                filePath
            );
            return 1;
        }

        // Generate command using T4 template
        var generatedCode = GenerateCommand(commandName);
        File.WriteAllText(filePath, generatedCode);
        AnsiConsole.MarkupLine($"[green]✓ Command '{commandName}' created at '{filePath}'.[/]");
        return 0;
    }

    private string NormalizeCommandName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Convert to PascalCase and remove invalid characters
        var words = name.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(
            words.Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1))
        );
    }

    private string GenerateCommand(string commandName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        // Get embedded resource names
        string[] resourceNames = assembly.GetManifestResourceNames();
        // Find the T4 template resource
        var templateResource = resourceNames.First(rn => rn.EndsWith("CommandTemplate.tt"));

        using Stream stream = assembly.GetManifestResourceStream(templateResource);
        using StreamReader reader = new StreamReader(stream);
        var templateContent = reader.ReadToEnd();

        // var templatePath = Path.Combine(
        //     Directory.GetCurrentDirectory(),
        //     "Templates",
        //     "CommandTemplate.tt"
        // );
        // if (!File.Exists(templatePath))
        // {
        //     throw new FileNotFoundException($"T4 template not found at: {templatePath}");
        // }

        // var templateContent = File.ReadAllText(templatePath);

        // Simple template replacement (basic implementation without full T4 processing)
        var result = templateContent
            .Replace("<#@ parameter name=\"CommandName\" type=\"System.String\" #>", "")
            .Replace("<#@ parameter name=\"Namespace\" type=\"System.String\" #>", "")
            .Replace("<#= CommandName #>", commandName)
            .Replace("<#= Namespace #>", "DotNet.Artisan");

        // Remove T4 directives for this simple implementation
        var lines = result.Split('\n').ToList();
        var codeLines = new List<string>();
        var skipLines = true;

        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("using"))
            {
                skipLines = false;
            }

            if (!skipLines && !line.TrimStart().StartsWith("<#@"))
            {
                codeLines.Add(line);
            }
        }

        return string.Join("\n", codeLines);
    }
}
