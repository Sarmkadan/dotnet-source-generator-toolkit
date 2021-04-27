# FilePathValidator

Provides utility methods for validating file paths, ensuring directory existence, and safely combining or resolving paths in a cross-platform manner.

## API

### `public static bool IsValidPath(string? path)`

Determines whether the provided string is a valid file or directory path.

- **Parameters**
  - `path` – The path string to validate; `null` is considered invalid.
- **Return value**
  - `true` if the path is non-null and adheres to platform-specific path format rules; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `path` is `null`.

---

### `public static bool EnsureDirectoryExists(string? path)`

Ensures that the directory containing the specified path exists, creating it if necessary.

- **Parameters**
  - `path` – The file or directory path whose parent directory should exist; `null` is treated as invalid.
- **Return value**
  - `true` if the directory already exists or was successfully created; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `path` is `null`.
  - Throws `IOException` if directory creation fails due to permissions or other I/O issues.

---

### `public static string? GetRelativePath(string? fromPath, string? toPath)`

Computes the relative path from one location to another, suitable for use in file system APIs.

- **Parameters**
  - `fromPath` – The base path from which the relative path is calculated; `null` is treated as invalid.
  - `toPath` – The target path to resolve relative to `fromPath`; `null` is treated as invalid.
- **Return value**
  - A relative path string if both inputs are valid and the resolution succeeds; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if either `fromPath` or `toPath` is `null`.

---

### `public static string? CombineSafePath(string? basePath, string? relativePath)`

Combines two path components safely, avoiding directory traversal and ensuring a normalized result.

- **Parameters**
  - `basePath` – The base directory path; `null` is treated as invalid.
  - `relativePath` – The relative path to append; `null` is treated as invalid.
- **Return value**
  - A combined, normalized path string if both inputs are valid; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if either `basePath` or `relativePath` is `null`.

---
### `public static string? GetDirectory(string? path)`

Extracts the directory portion of a file path, returning `null` if the input is invalid or represents a root.

- **Parameters**
  - `path` – The file or directory path from which to extract the directory; `null` is treated as invalid.
- **Return value**
  - The directory path if the input is valid and not a root; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `path` is `null`.

## Usage

```csharp
using System;
using DotNet.SourceGenerator.Toolkit;

class Example
{
    static void Main()
    {
        string baseDir = "/projects/myapp";
        string filePath = "/projects/myapp/src/Program.cs";

        // Validate and ensure directory exists
        if (FilePathValidator.IsValidPath(filePath) &&
            FilePathValidator.EnsureDirectoryExists(filePath))
        {
            Console.WriteLine("Path is valid and directory exists.");
        }

        // Compute relative path
        string relative = FilePathValidator.GetRelativePath(baseDir, filePath);
        Console.WriteLine($"Relative path: {relative}");
    }
}
```

```csharp
using System;
using DotNet.SourceGenerator.Toolkit;

class BuildTask
{
    public void CopyFile(string source, string destination)
    {
        if (FilePathValidator.IsValidPath(source) &&
            FilePathValidator.IsValidPath(destination))
        {
            string? dir = FilePathValidator.GetDirectory(destination);
            if (dir != null && FilePathValidator.EnsureDirectoryExists(dir))
            {
                string? combined = FilePathValidator.CombineSafePath(dir, "output.txt");
                if (combined != null)
                {
                    Console.WriteLine($"Copying to: {combined}");
                }
            }
        }
    }
}
```

## Notes

- **Edge Cases**: Methods return `null` for invalid inputs rather than throwing, except where documented. `EnsureDirectoryExists` may fail on read-only or locked directories.
- **Thread Safety**: All methods are stateless and thread-safe; no shared mutable state is accessed.
