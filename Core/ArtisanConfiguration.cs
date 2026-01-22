using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.Artisan.Core
{
    public interface IArtisanConfiguration
    {
        string CommandsPath { get; set; }
        bool EnableCommandDiscovery { get; set; }
        Dictionary<string, string> Aliases { get; set; }
        List<string> Providers { get; set; }
        Dictionary<string, string> Paths { get; set; }
        ArtisanLoggingConfiguration Logging { get; set; }
    }

    public class ArtisanConfiguration : IArtisanConfiguration
    {
        public string CommandsPath { get; set; } = "Console/Commands";
        public bool EnableCommandDiscovery { get; set; } = true;
        public Dictionary<string, string> Aliases { get; set; } = new();
        public List<string> Providers { get; set; } = new();
        public Dictionary<string, string> Paths { get; set; } =
            new()
            {
                ["commands"] = "Console/Commands",
                ["models"] = "Models",
                ["controllers"] = "Controllers",
                ["views"] = "Views",
            };
        public ArtisanLoggingConfiguration Logging { get; set; } = new();
    }

    public class ArtisanLoggingConfiguration
    {
        public string Level { get; set; } = "Information";
        public bool EnableColors { get; set; } = true;
        public bool EnableTimestamps { get; set; } = false;
    }

    public class ArtisanConfigurationManager
    {
        private static readonly string[] ConfigFileNames =
        {
            "artisan.json",
            "dotnet-artisan.json",
            ".artisan.json",
        };

        public static IArtisanConfiguration LoadConfiguration(string? projectPath = null)
        {
            projectPath ??= Directory.GetCurrentDirectory();

            foreach (var configFileName in ConfigFileNames)
            {
                var configPath = Path.Combine(projectPath, configFileName);
                if (File.Exists(configPath))
                {
                    return LoadConfigurationFromFile(configPath);
                }
            }

            // Return default configuration if no file found
            return new ArtisanConfiguration();
        }

        private static IArtisanConfiguration LoadConfigurationFromFile(string configPath)
        {
            try
            {
                var jsonContent = File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                };

                var configuration = JsonSerializer.Deserialize<ArtisanConfiguration>(
                    jsonContent,
                    options
                );
                return configuration ?? new ArtisanConfiguration();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Warning: Could not load artisan configuration from '{configPath}': {ex.Message}"
                );
                return new ArtisanConfiguration();
            }
        }

        public static void SaveConfiguration(
            IArtisanConfiguration configuration,
            string? projectPath = null
        )
        {
            projectPath ??= Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectPath, "artisan.json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            };

            var jsonContent = JsonSerializer.Serialize(configuration, options);
            File.WriteAllText(configPath, jsonContent);
        }

        public static string GetPath(IArtisanConfiguration configuration, string pathKey)
        {
            return configuration.Paths.TryGetValue(pathKey, out var path) ? path : string.Empty;
        }

        public static string GetCommandAlias(
            IArtisanConfiguration configuration,
            string commandName
        )
        {
            return configuration.Aliases.TryGetValue(commandName, out var alias)
                ? alias
                : commandName;
        }
    }

    /// <summary>
    /// Service provider interface for artisan commands
    /// </summary>
    public interface IArtisanServiceProvider
    {
        T GetService<T>()
            where T : class;
        IEnumerable<T> GetServices<T>()
            where T : class;
    }

    /// <summary>
    /// Base interface for artisan service providers
    /// </summary>
    public interface IArtisanProvider
    {
        void Register(IServiceCollection services);
        void ConfigureServices(IArtisanConfiguration configuration);
    }
}
