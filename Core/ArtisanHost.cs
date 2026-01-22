using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace DotNet.Artisan.Core
{
    public interface IArtisanBuilder
    {
        IArtisanBuilder AddCommand<T>(string? name = null)
            where T : class, ICommand;
        IArtisanBuilder UseCommandDiscovery(string path = "Console/Commands");
        IArtisanBuilder ConfigureApp(Action<IConfigurationBuilder> configure);
        IArtisanBuilder ConfigureLogging(Action<ILoggingBuilder> configure);
        IArtisanBuilder ConfigureServices(Action<IServiceCollection> configure);
        CommandApp Build();
    }

    public class ArtisanBuilder : IArtisanBuilder
    {
        private readonly string[] _args;
        private readonly string _projectPath;
        private readonly ProjectContext _context;
        private readonly List<(Type, string?)> _commandTypes = new();
        private string _commandDiscoveryPath = "Console/Commands";
        private Action<IConfigurationBuilder>? _configureApp;
        private Action<ILoggingBuilder>? _configureLogging;
        private Action<IServiceCollection>? _configureServices;

        public ArtisanBuilder(string[] args, string? projectPath = null)
        {
            _args = args;
            _projectPath = projectPath ?? Directory.GetCurrentDirectory();
            _context = ProjectContextDetector.DetectContext(_projectPath);
        }

        public IArtisanBuilder AddCommand<T>(string? name = null)
            where T : class, ICommand
        {
            _commandTypes.Add((typeof(T), name));
            return this;
        }

        public IArtisanBuilder UseCommandDiscovery(string path = "Console/Commands")
        {
            _commandDiscoveryPath = path;
            return this;
        }

        public IArtisanBuilder ConfigureApp(Action<IConfigurationBuilder> configure)
        {
            _configureApp = configure;
            return this;
        }

        public IArtisanBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            _configureLogging = configure;
            return this;
        }

        public IArtisanBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            _configureServices = configure;
            return this;
        }

        public CommandApp Build()
        {
            var app = new CommandApp();

            app.Configure(config =>
            {
                // Register explicitly added commands
                RegisterExplicitCommands(config);

                // Register discovered commands
                RegisterDiscoveredCommands(config);
            });

            return app;
        }

        private void RegisterExplicitCommands(IConfigurator config)
        {
            foreach (var commandType in _commandTypes)
            {
                var commandName =
                    commandType.Item2
                    ?? commandType.Item1.Name.Replace("Command", "").ToLowerInvariant();
                try
                {
                    var addCommandMethod = config.GetType().GetMethod("AddCommand");
                    var genericMethod = addCommandMethod?.MakeGenericMethod(commandType.Item1);
                    genericMethod?.Invoke(config, new object[] { commandName });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Warning: Could not register command '{commandName}': {ex.Message}"
                    );
                }
            }
        }

        private void RegisterDiscoveredCommands(IConfigurator config)
        {
            var discoveryService = new CommandDiscoveryService();
            var discoveredCommands = discoveryService.DiscoverCommands(_commandDiscoveryPath);

            foreach (var commandType in discoveredCommands)
            {
                var commandName = commandType.Name.Replace("Command", "").ToLowerInvariant();
                try
                {
                    var addCommandMethod = config.GetType().GetMethod("AddCommand");
                    var genericMethod = addCommandMethod?.MakeGenericMethod(commandType);
                    genericMethod?.Invoke(config, new object[] { commandName });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Warning: Could not register discovered command '{commandName}': {ex.Message}"
                    );
                }
            }
        }
    }

    public static class ArtisanHost
    {
        public static IArtisanBuilder CreateBuilder(string[] args, string? projectPath = null)
        {
            return new ArtisanBuilder(args, projectPath);
        }

        public static IArtisanBuilder CreateBuilder(string projectPath = null)
        {
            return new ArtisanBuilder(Array.Empty<string>(), projectPath);
        }

        public static int Run(string[] args, string? projectPath = null)
        {
            var app = CreateBuilder(args, projectPath).Build();
            return app.Run(args);
        }
    }

    public class ArtisanOptions
    {
        public string CommandsPath { get; set; } = "Console/Commands";
        public bool EnableCommandDiscovery { get; set; } = true;
        public Dictionary<string, string> Aliases { get; set; } = new();
    }
}
