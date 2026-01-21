using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

internal class MakeModel : Command<MakeModel.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[name]")]
        [Description("The name of the model to create.")]
        public string Name { get; init; } = string.Empty;
    }

    public override int Execute(
        CommandContext context,
        Settings settings,
        CancellationToken cancellation
    )
    {
        var modelName = settings.Name;
        while (
            string.IsNullOrEmpty(modelName)
            || modelName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
        )
        {
            if (!string.IsNullOrEmpty(modelName))
            {
                AnsiConsole.MarkupLine(
                    "[red]ERROR:[/] Model name [yellow]'{0}'[/] contains invalid characters. Please try again.",
                    modelName
                );
            }
            modelName = AnsiConsole.Ask<string>("What the model should be called?");
        }
        var currentDirectory = Directory.GetCurrentDirectory();
        var modelsDirectory = Path.Combine(currentDirectory, "Models");
        if (!Directory.Exists(modelsDirectory))
        {
            Directory.CreateDirectory(modelsDirectory);
        }
        var filePath = Path.Combine(currentDirectory, "Models", $"{modelName}.cs");
        if (File.Exists(filePath))
        {
            AnsiConsole.MarkupLine(
                "[red]ERROR:[/] Model [yellow]'{0}'[/] already exists at '{1}'.",
                modelName,
                filePath
            );
            return 1;
        }
        File.WriteAllText(
            filePath,
            $"using System;\npublic class {modelName}\n{{\n    // Define properties here\n    public int Id {{ get; set; }}\n}}\n"
        );
        AnsiConsole.MarkupLine($"[green]✓ Model '{modelName}' created at '{filePath}'.[/]");
        return 0;
    }
}
