using System.ComponentModel;
using Spectre.Console.Cli;

#nullable disable

internal class Testcommand : BaseCommand<Testcommand.Settings>
{
    public class Settings : CommandSettings
    {
        // Add your command arguments and options here
        // Example:
        // [CommandArgument(0, "<name>")]
        // [Description("The name to process.")]
        // public string Name { get; init; } = string.Empty;
    }

    public override int Execute(
        CommandContext context,
        Settings settings,
        CancellationToken cancellation
    )
    {
        Info("Executing Testcommand command...");

        // Demonstrate the helper methods
        var items = new[]
        {
            "Processing file 1",
            "Processing file 2",
            "Processing file 3",
            "Processing file 4",
            "Processing file 5",
        };

        WithProgressBar(
            items,
            item =>
            {
                // Simulate some work
                Thread.Sleep(300);
            }
        );

        Info("Command completed successfully!");
        return 0;
    }
}
