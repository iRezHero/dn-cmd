using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNet.Artisan.Core
{
    public enum ProjectType
    {
        Console,
        AspNetCore,
        WorkerService,
        ClassLibrary,
        Generic
    }

    public class ProjectContext
    {
        public ProjectType ProjectType { get; set; }
        public string ProjectPath { get; set; } = string.Empty;
        public string ProjectFile { get; set; } = string.Empty;
        public Dictionary<string, string> Properties { get; set; } = new();
        public bool HasConfiguration { get; set; }
        public bool HasDbContext { get; set; }
        public List<string> AvailablePackages { get; set; } = new();
    }

    public class ProjectContextDetector
    {
        public static ProjectContext DetectContext(string projectPath = null)
        {
            projectPath ??= Directory.GetCurrentDirectory();
            
            var context = new ProjectContext
            {
                ProjectPath = projectPath,
                ProjectFile = FindProjectFile(projectPath)
            };

            if (!string.IsNullOrEmpty(context.ProjectFile))
            {
                context = AnalyzeProjectFile(context.ProjectFile);
                context.Properties = ExtractProjectProperties(context.ProjectFile);
            }

            // Detect additional features
            context.HasConfiguration = HasConfigurationFiles(projectPath);
            context.HasDbContext = HasDbContext(projectPath);
            context.AvailablePackages = GetAvailablePackages(projectPath);

            return context;
        }

        private static string FindProjectFile(string projectPath)
        {
            var csprojFiles = Directory.GetFiles(projectPath, "*.csproj");
            if (csprojFiles.Length == 1)
                return csprojFiles[0];

            if (csprojFiles.Length > 1)
            {
                // Try to find the main project (not test projects)
                var mainProject = csprojFiles.FirstOrDefault(f => 
                    !Path.GetFileName(f).Contains(".Test") &&
                    !Path.GetFileName(f).Contains(".Tests"));
                return mainProject ?? csprojFiles[0];
            }

            return string.Empty;
        }

        private static ProjectContext AnalyzeProjectFile(string projectFile)
        {
            var content = File.ReadAllText(projectFile);
            var context = new ProjectContext
            {
                ProjectFile = projectFile,
                ProjectPath = Path.GetDirectoryName(projectFile) ?? string.Empty
            };

            // Detect project type based on SDK and packages
            if (content.Contains("Microsoft.NET.Sdk.Web"))
            {
                context.ProjectType = ProjectType.AspNetCore;
            }
            else if (content.Contains("Microsoft.NET.Sdk.Worker"))
            {
                context.ProjectType = ProjectType.WorkerService;
            }
            else if (content.Contains("Microsoft.NET.Sdk"))
            {
                if (content.Contains("<OutputType>Exe</OutputType>") ||
                    content.Contains("<OutputType>WinExe</OutputType>"))
                {
                    context.ProjectType = ProjectType.Console;
                }
                else
                {
                    context.ProjectType = ProjectType.ClassLibrary;
                }
            }
            else
            {
                context.ProjectType = ProjectType.Generic;
            }

            return context;
        }

        private static Dictionary<string, string> ExtractProjectProperties(string projectFile)
        {
            var properties = new Dictionary<string, string>();
            var content = File.ReadAllText(projectFile);

            // Extract TargetFramework
            var targetFrameworkMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"<TargetFramework>(.*?)</TargetFramework>");
            if (targetFrameworkMatch.Success)
            {
                properties["TargetFramework"] = targetFrameworkMatch.Groups[1].Value.Trim();
            }

            // Extract AssemblyName
            var assemblyNameMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"<AssemblyName>(.*?)</AssemblyName>");
            if (assemblyNameMatch.Success)
            {
                properties["AssemblyName"] = assemblyNameMatch.Groups[1].Value.Trim();
            }

            // Extract RootNamespace
            var rootNamespaceMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"<RootNamespace>(.*?)</RootNamespace>");
            if (rootNamespaceMatch.Success)
            {
                properties["RootNamespace"] = rootNamespaceMatch.Groups[1].Value.Trim();
            }

            return properties;
        }

        private static bool HasConfigurationFiles(string projectPath)
        {
            var configFiles = new[]
            {
                "appsettings.json",
                "appsettings.Development.json",
                "appsettings.Production.json",
                "config.json",
                "settings.json"
            };

            return configFiles.Any(config => File.Exists(Path.Combine(projectPath, config)));
        }

        private static bool HasDbContext(string projectPath)
        {
            // Check for common EF Core patterns
            var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
            
            return csFiles.Any(file =>
            {
                var content = File.ReadAllText(file);
                return content.Contains("DbContext") && 
                       (content.Contains("Microsoft.EntityFrameworkCore") || 
                        content.Contains(": DbContext"));
            });
        }

        private static List<string> GetAvailablePackages(string projectPath)
        {
            var packages = new List<string>();
            
            if (File.Exists(Path.Combine(projectPath, "packages.config")))
            {
                // Old packages.config format
                var content = File.ReadAllText(Path.Combine(projectPath, "packages.config"));
                var matches = System.Text.RegularExpressions.Regex.Matches(
                    content, @"id=""(.*?)""");
                packages.AddRange(matches.Cast<System.Text.RegularExpressions.Match>()
                    .Select(m => m.Groups[1].Value));
            }

            var projectFile = FindProjectFile(projectPath);
            if (!string.IsNullOrEmpty(projectFile))
            {
                var content = File.ReadAllText(projectFile);
                var matches = System.Text.RegularExpressions.Regex.Matches(
                    content, @"PackageReference.*Include=""(.*?)""");
                packages.AddRange(matches.Cast<System.Text.RegularExpressions.Match>()
                    .Select(m => m.Groups[1].Value));
            }

            return packages.Distinct().ToList();
        }
    }
}