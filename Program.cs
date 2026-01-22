using System;
using System.IO;
using DotNet.Artisan.Commands;
using DotNet.Artisan.Core;
using Spectre.Console.Cli;

namespace DotNet.Artisan
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine();

            // Load artisan configuration
            var configuration = ArtisanConfigurationManager.LoadConfiguration();

            // Create artisan host with configuration
            var app = ArtisanHost
                .CreateBuilder(args)
                .AddCommand<MakeModel>("make:model")
                .AddCommand<MakeCommand>("make:command")
                .UseCommandDiscovery(configuration.CommandsPath)
                .Build();

            return app.Run(args);
        }
    }
}
