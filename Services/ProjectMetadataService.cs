// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Xml.Linq;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Extracts and manages metadata from .NET project files including
/// dependencies, framework targets, and project configuration.
/// </summary>
public interface IProjectMetadataService
{
    /// <summary>Reads project file metadata (.csproj).</summary>
    Task<ProjectMetadata> ReadProjectMetadataAsync(string projectPath);

    /// <summary>Gets NuGet dependencies from the project.</summary>
    Task<IEnumerable<Dependency>> GetDependenciesAsync(string projectPath);

    /// <summary>Gets target framework information.</summary>
    Task<string> GetTargetFrameworkAsync(string projectPath);

    /// <summary>Gets root namespace of the project.</summary>
    Task<string> GetRootNamespaceAsync(string projectPath);

    /// <summary>Validates project file exists and is valid.</summary>
    Task<bool> ValidateProjectAsync(string projectPath);
}

/// <summary>
/// Represents project metadata extracted from project file.
/// </summary>
public class ProjectMetadata
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = string.Empty;
    public string RootNamespace { get; set; } = string.Empty;
    public Version? ProjectVersion { get; set; }
    public List<Dependency> Dependencies { get; } = [];
    public Dictionary<string, string> Properties { get; } = [];
    public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a NuGet package dependency.
/// </summary>
public class Dependency
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsDevDependency { get; set; }
}

/// <summary>
/// Extracts metadata from .NET projects including frameworks and dependencies.
/// </summary>
public class ProjectMetadataService : IProjectMetadataService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<ProjectMetadataService> _logger;
    private const string CSPROJ_EXTENSION = ".csproj";

    public ProjectMetadataService(IFileSystemService fileSystemService, ILogger<ProjectMetadataService> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    public async Task<ProjectMetadata> ReadProjectMetadataAsync(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentNullException(nameof(projectPath));

        _logger.LogInformation("Reading project metadata from: {ProjectPath}", projectPath);

        var metadata = new ProjectMetadata { ProjectPath = projectPath };

        try
        {
            var csprojFile = FindCsprojFile(projectPath);
            if (string.IsNullOrEmpty(csprojFile))
                throw new InvalidOperationException("Project file (.csproj) not found");

            var projectContent = await _fileSystemService.ReadFileAsync(csprojFile);
            var doc = XDocument.Parse(projectContent);
            var root = doc.Root;

            // Extract project name
            metadata.ProjectName = new DirectoryInfo(projectPath).Name;

            // Extract target framework
            var targetFramework = root?.Element("PropertyGroup")?.Element("TargetFramework")?.Value;
            metadata.TargetFramework = targetFramework ?? "net10.0";

            // Extract root namespace
            var rootNamespace = root?.Element("PropertyGroup")?.Element("RootNamespace")?.Value;
            metadata.RootNamespace = rootNamespace ?? metadata.ProjectName;

            // Extract version
            var version = root?.Element("PropertyGroup")?.Element("Version")?.Value;
            if (!string.IsNullOrEmpty(version) && Version.TryParse(version, out var parsedVersion))
                metadata.ProjectVersion = parsedVersion;

            // Extract dependencies
            var itemGroup = root?.Elements("ItemGroup").FirstOrDefault();
            if (itemGroup != null)
            {
                foreach (var pkgRef in itemGroup.Elements("PackageReference"))
                {
                    var depName = pkgRef.Attribute("Include")?.Value;
                    var depVersion = pkgRef.Attribute("Version")?.Value;

                    if (!string.IsNullOrEmpty(depName))
                    {
                        metadata.Dependencies.Add(new Dependency
                        {
                            Name = depName,
                            Version = depVersion ?? "latest",
                        });
                    }
                }
            }

            _logger.LogInformation("Loaded project metadata for: {ProjectName} (TFM: {TargetFramework})",
                metadata.ProjectName, metadata.TargetFramework);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading project metadata");
            throw;
        }

        return await Task.FromResult(metadata);
    }

    public async Task<IEnumerable<Dependency>> GetDependenciesAsync(string projectPath)
    {
        var metadata = await ReadProjectMetadataAsync(projectPath);
        return metadata.Dependencies;
    }

    public async Task<string> GetTargetFrameworkAsync(string projectPath)
    {
        var metadata = await ReadProjectMetadataAsync(projectPath);
        return await Task.FromResult(metadata.TargetFramework);
    }

    public async Task<string> GetRootNamespaceAsync(string projectPath)
    {
        var metadata = await ReadProjectMetadataAsync(projectPath);
        return await Task.FromResult(metadata.RootNamespace);
    }

    public async Task<bool> ValidateProjectAsync(string projectPath)
    {
        if (!Directory.Exists(projectPath))
        {
            _logger.LogWarning("Project directory not found: {ProjectPath}", projectPath);
            return false;
        }

        var csprojFile = FindCsprojFile(projectPath);
        if (string.IsNullOrEmpty(csprojFile))
        {
            _logger.LogWarning("Project file (.csproj) not found in: {ProjectPath}", projectPath);
            return false;
        }

        try
        {
            var content = await _fileSystemService.ReadFileAsync(csprojFile);
            XDocument.Parse(content);
            _logger.LogInformation("Project validation passed for: {ProjectPath}", projectPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Project validation failed for: {ProjectPath}", projectPath);
            return false;
        }
    }

    private string FindCsprojFile(string projectPath)
    {
        var files = Directory.GetFiles(projectPath, $"*{CSPROJ_EXTENSION}", SearchOption.TopDirectoryOnly);

        if (files.Length == 0)
            return string.Empty;

        if (files.Length == 1)
            return files[0];

        // If multiple .csproj files, prefer one matching directory name
        var dirName = new DirectoryInfo(projectPath).Name;
        var matching = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(dirName, StringComparison.OrdinalIgnoreCase));

        return matching ?? files[0];
    }
}
