using System.ComponentModel;
using Spectre.Console.Cli;

internal class Errordemo : BaseCommand<Errordemo.Settings>
{
    public class Settings : CommandSettings
    {
        // Add your command arguments and options here
        // Example:
        // [CommandArgument(0, "<name>")]
        // [Description("The name to process.")]
        // public string Name { get; init; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        Info("Executing Errordemo command...");
        
        // Your command logic goes here
        // Use this.Info() for green messages
        // Use this.Error() for red messages
        // Use this.WithProgressBar() for progress bars
        
        // Example:
        // var items = new[] { "Item 1", "Item 2", "Item 3" };
        // WithProgressBar(items, item => {
        //     Info($"Processing {item}");
        //     Thread.Sleep(500); // Simulate work
        // });
        
        Info("Command completed successfully!");
        return 0;
    }
}