# FileSystemServiceExtensions

Provides a set of static extension methods that simplify common file-system operations with asynchronous execution, safe deletion semantics, and path manipulation utilities. This type serves as a thin, consistent abstraction over `System.IO` and `System.Threading.Tasks` to reduce boilerplate in source-generation and build-time tooling scenarios.

## API

### ReadAllLinesAsync
```csharp
public static async Task<IReadOnlyList<string>> ReadAllLinesAsync(string path)
```
Reads all lines from the specified file asynchronously.
- **path**: Absolute or relative path to the file.
- **Returns**: A read-only list of strings, each representing one line in the file.
- **Throws**: `ArgumentNullException` when `path` is `null`; `FileNotFoundException` when the file does not exist; `IOException` when an I/O error occurs.

### WriteAllLinesAsync
```csharp
public static async Task WriteAllLinesAsync(string path, IEnumerable<string> lines)
```
Writes a sequence of strings to a file asynchronously, overwriting the file if it already exists.
- **path**: Absolute or relative path to the target file.
- **lines**: The lines to write.
- **Throws**: `ArgumentNullException` when `path` or `lines` is `null`; `DirectoryNotFoundException` when the target directory does not exist; `IOException` when an I/O error occurs.

### DirectoryExists
```csharp
public static bool DirectoryExists(string path)
```
Determines whether the specified directory exists.
- **path**: Absolute or relative path to the directory.
- **Returns**: `true` if the directory exists; otherwise `false`.
- **Throws**: `ArgumentNullException` when `path` is `null`.

### SafeDeleteFileAsync
```csharp
public static async Task<bool> SafeDeleteFileAsync(string filePath)
```
Attempts to delete a file asynchronously without throwing if the file is missing or inaccessible.
- **filePath**: Absolute or relative path to the file.
- **Returns**: `true` if the file was successfully deleted; `false` if the file did not exist or could not be deleted.
- **Throws**: `ArgumentNullException` when `filePath` is `null`.

### SafeDeleteDirectoryAsync
```csharp
public static async Task<bool> SafeDeleteDirectoryAsync(string directoryPath)
```
Attempts to delete a directory recursively asynchronously without throwing if the directory is missing or inaccessible.
- **directoryPath**: Absolute or relative path to the directory.
- **Returns**: `true` if the directory was successfully deleted; `false` if it did not exist or could not be deleted.
- **Throws**: `ArgumentNullException` when `directoryPath` is `null`.

### ReadAllTextAsync
```csharp
public static async Task<string> ReadAllTextAsync(string filePath)
```
Reads all text from a file asynchronously.
- **filePath**: Absolute or relative path to the file.
- **Returns**: The entire file contents as a string.
- **Throws**: `ArgumentNullException` when `filePath` is `null`; `FileNotFoundException` when the file does not exist; `IOException` when an I/O error occurs.

### GetFileName
```csharp
public static string GetFileName(string path)
```
Returns the file name and extension from the given path.
- **path**: A file path.
- **Returns**: The file name with extension (e.g., `"document.txt"`).
- **Throws**: `ArgumentNullException` when `path` is `null`.

### GetFileNameWithoutExtension
```csharp
public static string GetFileNameWithoutExtension(string path)
```
Returns the file name without the extension.
- **path**: A file path.
- **Returns**: The file name without extension (e.g., `"document"`).
- **Throws**: `ArgumentNullException` when `path` is `null`.

### GetFileExtension
```csharp
public static string GetFileExtension(string path)
```
Returns the file extension including the leading period.
- **path**: A file path.
- **Returns**: The extension (e.g., `".txt"`). Returns `string.Empty` if no extension is present.
- **Throws**: `ArgumentNullException` when `path` is `null`.

### GetRelativePath
```csharp
public static string GetRelativePath(string relativeTo, string path)
```
Computes a relative path from one location to another.
- **relativeTo**: The base directory or file path.
- **path**: The target file or directory path.
- **Returns**: The relative path string.
- **Throws**: `ArgumentNullException` when either argument is `null`; `ArgumentException` when paths are on different roots.

### GetFullPath
```csharp
public static string GetFullPath(string path)
```
Converts a relative path to an absolute path.
- **path**: A relative or absolute path.
- **Returns**: The fully qualified absolute path.
- **Throws**: `ArgumentNullException` when `path` is `null`.

## Usage

### Example 1: Reading, transforming, and safely writing a file
```csharp
string inputPath = FileSystemServiceExtensions.GetFullPath("data/input.txt");
string outputPath = FileSystemServiceExtensions.GetFullPath("data/output.txt");

if (File.Exists(inputPath))
{
    IReadOnlyList<string> lines = await FileSystemServiceExtensions.ReadAllLinesAsync(inputPath);
    var transformed = lines.Select(l => l.ToUpperInvariant());
    await FileSystemServiceExtensions.WriteAllLinesAsync(outputPath, transformed);
}
```

### Example 2: Safe cleanup of a generated directory
```csharp
string genDir = FileSystemServiceExtensions.GetFullPath("generated");

if (FileSystemServiceExtensions.DirectoryExists(genDir))
{
    bool deleted = await FileSystemServiceExtensions.SafeDeleteDirectoryAsync(genDir);
    if (!deleted)
    {
        Console.WriteLine("Cleanup skipped or failed silently.");
    }
}
```

## Notes

- **Safe deletion semantics**: `SafeDeleteFileAsync` and `SafeDeleteDirectoryAsync` do not throw when the target is missing or cannot be deleted due to permissions or locks. The return value indicates success; callers must inspect it when failure matters.
- **Path normalization**: `GetFullPath` resolves relative paths against the current working directory. Use it to normalize user-supplied paths before passing them to other methods.
- **Thread safety**: All static methods are inherently thread-safe as they operate on independent file-system state. Concurrent writes to the same file path from multiple threads may result in interleaved or corrupted content; external synchronization is the caller's responsibility.
- **Asynchronous I/O**: The `async` methods use `Task`-based I/O internally. They should be awaited to avoid blocking the calling thread. They do not configure `ConfigureAwait(false)`, so callers in library code should explicitly configure continuation context if needed.
- **Edge cases for path methods**: `GetFileName` on a path ending with a directory separator returns an empty string. `GetFileExtension` on a file with no extension returns `null`. `GetRelativePath` throws when the two paths reside on different volume roots (e.g., `C:\` vs `D:\` on Windows).
