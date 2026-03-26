// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Represents a C# source file with its metadata, content, and processing state.
/// Used for both analyzing existing files and tracking generated artifacts.
/// </summary>
public class SourceFile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string FilePath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FileContent { get; set; } = string.Empty;

    public string ProjectPath { get; set; } = string.Empty;

    public List<string> Usings { get; } = [];

    public List<string> Namespaces { get; } = [];

    public List<string> TypeNames { get; } = [];

    public List<string> Attributes { get; } = [];

    public int LineCount { get; set; }

    public long FileSizeBytes { get; set; }

    public SourceFileType FileType { get; set; } = SourceFileType.CSharp;

    public SourceFileStatus Status { get; set; } = SourceFileStatus.Created;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public string? GeneratedFrom { get; set; }

    public bool IsGenerated { get; set; }

    public Dictionary<string, object> ParsedMetadata { get; } = [];

    /// <summary>
    /// Analyzes the file content and extracts metadata.
    /// </summary>
    public void AnalyzeContent()
    {
        if (string.IsNullOrEmpty(FileContent))
            return;

        LineCount = FileContent.Split(Environment.NewLine).Length;
        FileSizeBytes = FileContent.Length;

        // Extract using statements
        var lines = FileContent.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("using "))
            {
                var usingName = trimmed["using ".Length..].TrimEnd(';');
                Usings.Add(usingName);
            }
            else if (trimmed.StartsWith("namespace "))
            {
                var namespaceName = trimmed["namespace ".Length..].TrimEnd('{', ';');
                if (!Namespaces.Contains(namespaceName))
                    Namespaces.Add(namespaceName);
            }
            else if (trimmed.StartsWith("public class ") || trimmed.StartsWith("public struct ") ||
                     trimmed.StartsWith("public interface ") || trimmed.StartsWith("public enum "))
            {
                var parts = trimmed.Split(' ');
                if (parts.Length > 2)
                {
                    var typeName = parts[2].Split(new[] { ':', '(' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    TypeNames.Add(typeName);
                }
            }
        }
    }

    /// <summary>
    /// Adds a using statement if not already present.
    /// </summary>
    public void AddUsing(string usingNamespace)
    {
        if (!string.IsNullOrWhiteSpace(usingNamespace) && !Usings.Contains(usingNamespace))
            Usings.Add(usingNamespace);
    }

    /// <summary>
    /// Checks if the file has the specified using statement.
    /// </summary>
    public bool HasUsing(string usingNamespace) => Usings.Contains(usingNamespace);

    /// <summary>
    /// Prepends using statements to the file content.
    /// </summary>
    public void PrependUsings()
    {
        if (Usings.Count == 0)
            return;

        var usingLines = Usings.Distinct().Select(u => $"using {u};");
        var allUsings = string.Join(Environment.NewLine, usingLines);
        FileContent = allUsings + Environment.NewLine + Environment.NewLine + FileContent;

        AnalyzeContent();
    }

    /// <summary>
    /// Validates the source file integrity.
    /// </summary>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(FileName))
            errors.Add("File name is required.");

        if (string.IsNullOrWhiteSpace(FilePath))
            errors.Add("File path is required.");

        if (!FileName.EndsWith(".cs"))
            errors.Add("File must be a C# source file (.cs).");

        if (string.IsNullOrWhiteSpace(FileContent))
            errors.Add("File content cannot be empty.");

        if (!FileContent.Contains("namespace "))
            errors.Add("File must declare a namespace.");

        return errors;
    }

    /// <summary>
    /// Gets a display-friendly relative path.
    /// </summary>
    public string GetRelativePath(string basePath)
    {
        if (FilePath.StartsWith(basePath))
            return FilePath[basePath.Length..].TrimStart('\\', '/');
        return FilePath;
    }
}

public enum SourceFileType
{
    CSharp,
    Json,
    Xml,
    Other,
}

public enum SourceFileStatus
{
    Created,
    Generated,
    Modified,
    Processed,
    Failed,
    Skipped,
}
