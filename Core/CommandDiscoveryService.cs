using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotNet.Artisan.Core
{
    public interface ICommandDiscovery
    {
        IEnumerable<Type> DiscoverCommands(string path);
        bool IsCommandType(Type type);
    }

    public class CommandDiscoveryService : ICommandDiscovery
    {
        public IEnumerable<Type> DiscoverCommands(string path)
        {
            var commands = new List<Type>();
            var searchPath = Path.Combine(Directory.GetCurrentDirectory(), path);

            if (!Directory.Exists(searchPath))
            {
                return commands;
            }

            // First try to load compiled assemblies from output directories
            var dllFiles = Directory.GetFiles(searchPath, "*.dll", SearchOption.AllDirectories);
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllFile);
                    var assemblyCommands = GetCommandTypes(assembly);
                    commands.AddRange(assemblyCommands);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Warning: Could not load assembly '{dllFile}': {ex.Message}"
                    );
                }
            }

            // If no DLLs found, try to compile CS files (simplified approach)
            if (!commands.Any())
            {
                var csFiles = Directory.GetFiles(searchPath, "*.cs", SearchOption.AllDirectories);
                foreach (var csFile in csFiles)
                {
                    try
                    {
                        var commandType = GetCommandTypeFromFile(csFile);
                        if (commandType != null)
                        {
                            commands.Add(commandType);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Warning: Could not analyze CS file '{csFile}': {ex.Message}"
                        );
                    }
                }
            }

            return commands.Distinct();
        }

        public bool IsCommandType(Type type)
        {
            return type.IsClass
                && !type.IsAbstract
                && typeof(Spectre.Console.Cli.ICommand).IsAssignableFrom(type);
        }

        private IEnumerable<Type> GetCommandTypes(Assembly assembly)
        {
            return assembly.GetTypes().Where(IsCommandType);
        }

        private Type? GetCommandTypeFromFile(string csFile)
        {
            // This is a simplified implementation
            // In a real scenario, you would use Roslyn to compile and analyze the file
            var content = File.ReadAllText(csFile);

            // Basic heuristics to determine if this is a command
            if (content.Contains("class ") && content.Contains(": Command<"))
            {
                // Extract class name
                var classMatch = System.Text.RegularExpressions.Regex.Match(
                    content,
                    @"class\s+(\w+).*?:\s*Command<"
                );

                if (classMatch.Success)
                {
                    // Return a placeholder type (in real implementation, you'd compile the file)
                    return typeof(object); // Placeholder
                }
            }

            return null;
        }
    }
}
