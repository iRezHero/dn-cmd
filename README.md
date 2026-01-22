# DotNet Artisan

A .NET CLI tool for creating artisan commands - Laravel-style CLI for .NET projects

## 🚀 Features

- **Laravel-like CLI** for .NET projects
- **Project Type Detection** automatically identifies Console, ASP.NET Core, Worker Service, or Class Library projects
- **Command Discovery** with convention-based loading from `Console/Commands` directory
- **Configuration System** via `artisan.json` file
- **Cross-platform** support (Windows, Linux, macOS)
- **NuGet Package** installable as global or local tool

## 📦 Installation

### As Global Tool
```bash
dotnet tool install -g DotNet.Artisan
```

### As Local Tool
```bash
cd YourProject
dotnet new tool-manifest
dotnet tool install DotNet.Artisan --local
```

### As Library Reference
Add to your `.csproj`:
```xml
<PackageReference Include="DotNet.Artisan" Version="1.0.0" />
```

## 🎯 Usage

### Basic Commands
```bash
# Create a new model
artisan make:model User

# Create a new command
artisan make:command SendEmails

# List available commands
artisan --help
```

### In Your Project
```bash
# Using dotnet tool run
dotnet tool run artisan make:model Product

# Using script (after setup)
./artisan make:model Order
```

## 📁 Project Structure

When you install DotNet.Artisan in your project, you can use this structure:

```
MyProject/
├── MyProject.csproj
├── artisan.json              # Configuration file
├── artisan                   # Script entry point
├── artisan.bat              # Windows script
├── Console/
│   └── Commands/           # Custom commands
│       ├── CreateUserCommand.cs
│       └── GenerateReportCommand.cs
├── Models/                 # Generated models
├── Controllers/            # Generated controllers (ASP.NET Core)
└── Views/                 # Generated views (ASP.NET Core)
```

## ⚙️ Configuration

Create an `artisan.json` file in your project root:

```json
{
  "commandsPath": "Console/Commands",
  "enableCommandDiscovery": true,
  "aliases": {
    "mm": "make:model",
    "mc": "make:command",
    "serve": "run"
  },
  "paths": {
    "commands": "Console/Commands",
    "models": "Models",
    "controllers": "Controllers",
    "views": "Views",
    "migrations": "Data/Migrations",
    "seeds": "Data/Seeds"
  },
  "logging": {
    "level": "Information",
    "enableColors": true,
    "enableTimestamps": false
  }
}
```

## 🔧 Creating Custom Commands

### 1. Using Built-in Generator
```bash
artisan make:command MyCustomCommand
```

This creates a template command in `Console/Commands/MyCustomCommand.cs` that inherits from `BaseCommand<TSettings>`.

### 2. Manual Command Creation
```csharp
using DotNet.Artisan.Core;
using Spectre.Console.Cli;

namespace DotNet.Artisan.Commands
{
    public class MyCustomCommand : BaseCommand<MyCustomCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[name]")]
            public string Name { get; set; } = string.Empty;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            Info($"Hello {settings.Name}!");
            return 0;
        }
    }
}
```

### 3. Using ArtisanHost in Code
```csharp
using DotNet.Artisan.Core;

// In your Program.cs
var app = ArtisanHost.CreateBuilder(args)
    .AddCommand<MyCustomCommand>()
    .UseCommandDiscovery("Console/Commands")
    .Build();

return app.Run(args);
```

## 🏗️ Project Context Awareness

DotNet Artisan automatically detects your project type:

- **ASP.NET Core** - Enables web-specific commands and templates
- **Console Application** - Console-appropriate commands
- **Worker Service** - Background service commands
- **Class Library** - Library-specific commands

## 📋 Built-in Commands

- `make:model` - Create a new model class
- `make:command` - Create a new artisan command

## 🎨 BaseCommand Features

When inheriting from `BaseCommand<TSettings>`, you get:

- **Colored Logging**: `Info()`, `Warn()`, `Debug()`, `Error()`
- **Progress Bars**: `WithProgressBar()` method
- **Context Awareness**: Access to project information

```csharp
public override int Execute(CommandContext context, Settings settings)
{
    Info("Processing files...");
    
    WithProgressBar(files, file => {
        // Process each file with progress bar
    });
    
    Warn("Operation completed!");
    return 0;
}
```

## 🔌 Advanced Features

### Service Providers
Register custom service providers in `artisan.json`:
```json
{
  "providers": [
    "MyProject.ArtisanProviders.DatabaseProvider",
    "MyProject.ArtisanProviders.CacheProvider"
  ]
}
```

### Aliases
Create custom command aliases:
```json
{
  "aliases": {
    "mm": "make:model",
    "ls": "list",
    "run": "serve"
  }
}
```

## 🚀 Publishing Your Extension

### As NuGet Package
1. Update your `.csproj` with package metadata
2. Build: `dotnet pack -c Release`
3. Publish: `dotnet nuget push YourPackage.nupkg`

### Project Detection
The tool automatically detects project types based on:
- `.csproj` SDK (`Microsoft.NET.Sdk.Web`, etc.)
- Package references (Entity Framework, etc.)
- Configuration files (`appsettings.json`)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Add your commands to `Console/Commands/`
4. Test with `dotnet run -- [command]`
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

Inspired by [Laravel Artisan](https://laravel.com/docs/10.x/artisan) 
and built with [Spectre.Console](https://spectreconsole.net/).

---

**Happy Coding! 🎉**