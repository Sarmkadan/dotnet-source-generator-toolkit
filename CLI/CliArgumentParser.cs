// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.CLI;

/// <summary>
/// Parses command-line arguments into CliOptions using a simple but robust approach.
/// Supports both long-form (--option value) and short-form (-o value) arguments.
/// </summary>
public class CliArgumentParser : ICliArgumentParser
{
    private const string Version = "1.0.0";
    private const string ProjectName = ".NET Source Generator Toolkit";

    public CliOptions Parse(string[] args)
    {
        var options = new CliOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            // Handle flags
            if (arg == "-h" || arg == "--help")
            {
                options.ShowHelp = true;
            }
            else if (arg == "-v" || arg == "--version")
            {
                options.ShowVersion = true;
            }
            else if (arg == "--verbose")
            {
                options.Verbose = true;
            }
            else if (arg == "--dry-run")
            {
                options.DryRun = true;
            }
            else if (arg == "--validate-only")
            {
                options.ValidateOnly = true;
            }
            else if (arg == "--generate-dtos")
            {
                options.GenerateDtos = true;
            }
            else if (arg == "--no-recursive")
            {
                options.Recursive = false;
            }

            // Handle options with values
            else if ((arg == "-p" || arg == "--path") && i + 1 < args.Length)
            {
                options.ProjectPath = args[++i];
            }
            else if ((arg == "-o" || arg == "--output") && i + 1 < args.Length)
            {
                options.OutputPath = args[++i];
            }
            else if ((arg == "-f" || arg == "--format") && i + 1 < args.Length)
            {
                options.OutputFormat = args[++i];
            }
            else if (arg == "--generators" && i + 1 < args.Length)
            {
                var generators = args[++i].Split(',', StringSplitOptions.TrimEntries);
                options.GeneratorTypes.AddRange(generators);
            }
            else if ((arg == "-n" || arg == "--namespace") && i + 1 < args.Length)
            {
                options.NamespaceOverride = args[++i];
            }
            else if ((arg == "-c" || arg == "--config") && i + 1 < args.Length)
            {
                options.ConfigFile = args[++i];
            }
            else if (arg == "--parallelism" && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i], out var degree))
                {
                    options.DegreeOfParallelism = Math.Max(1, degree);
                }
            }

            // Positional argument: project path
            else if (!arg.StartsWith("-") && string.IsNullOrEmpty(options.ProjectPath))
            {
                options.ProjectPath = arg;
            }
        }

        return options;
    }

    public string GetHelpMessage()
    {
        return @"
╔════════════════════════════════════════════════════════════════════════════╗
║                  .NET Source Generator Toolkit - Help                       ║
╚════════════════════════════════════════════════════════════════════════════╝

USAGE:
    dotnet-source-generator-toolkit [options] [project-path]

ARGUMENTS:
    [project-path]              Path to .NET project (default: current directory)

OPTIONS:
    -h, --help                  Show this help message
    -v, --version               Show version information
    -p, --path <path>           Project path (same as positional argument)
    -o, --output <path>         Output directory for generated files
    -f, --format <format>       Output format: Text, Json, Csv, Xml (default: Text)
    --generators <list>         Comma-separated list of generators to run
                                (Repository, Mapper, Validator, Serializer)
    -n, --namespace <namespace> Override namespace for generated code
    -c, --config <file>         Path to configuration file
    --parallelism <number>      Degree of parallelism (default: CPU count)
    --verbose                   Enable verbose logging output
    --dry-run                   Analyze without writing files
    --validate-only             Validate configuration without generation
    --generate-dtos             Generate DTOs alongside other artifacts
    --no-recursive              Don't search subdirectories

EXAMPLES:
    dotnet-source-generator-toolkit
    dotnet-source-generator-toolkit --path /path/to/project --verbose
    dotnet-source-generator-toolkit --format Json --output ./Generated
    dotnet-source-generator-toolkit --generators Repository,Mapper --dry-run
";
    }

    public string GetVersionInfo()
    {
        return $"{ProjectName} v{Version}";
    }

    public IEnumerable<string> Validate(CliOptions options)
    {
        var errors = new List<string>();

        if (!Directory.Exists(options.ProjectPath))
        {
            errors.Add($"Project path does not exist: {options.ProjectPath}");
        }

        if (!string.IsNullOrEmpty(options.OutputPath) && !Directory.Exists(options.OutputPath))
        {
            // OutputPath will be created if it doesn't exist
            try
            {
                Directory.CreateDirectory(options.OutputPath);
            }
            catch (Exception ex)
            {
                errors.Add($"Cannot create output directory: {ex.Message}");
            }
        }

        var validFormats = new[] { "Text", "Json", "Csv", "Xml" };
        if (!validFormats.Contains(options.OutputFormat, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"Invalid output format: {options.OutputFormat}");
        }

        if (options.DegreeOfParallelism < 1)
        {
            errors.Add("Degree of parallelism must be >= 1");
        }

        return errors;
    }
}
