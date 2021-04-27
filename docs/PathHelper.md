# PathHelper

Utility class providing path manipulation and normalization methods for working with file system paths in .NET applications and tooling.

## API

### `public static string NormalizePath(string path)`

Normalizes a file system path by converting separators to the current platform's preferred separator and removing redundant separators or relative segments.

- **Parameters**:
  - `path`: The path to normalize. Must not be `null`.
- **Return value**: The normalized path.
- **Throws**: `ArgumentNullException` if `path` is `null`.

---

### `public static string? ToRelativePath(string basePath, string path)`

Computes the relative path from `basePath` to `path`, if possible.

- **Parameters**:
  - `basePath`: The base directory path. Must not be `null`.
  - `path`: The target path. Must not be `null`.
- **Return value**: The relative path from `basePath` to `path`, or `null` if the paths are on different drives or otherwise incompatible.
- **Throws**: `ArgumentNullException` if either `basePath` or `path` is `null`.

---

### `public static bool IsAbsolute(string path)`

Determines whether the specified path is an absolute path.

- **Parameters**:
  - `path`: The path to check. Must not be `null`.
- **Return value**: `true` if the path is absolute; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `path` is `null`.

---

### `public static string? GetCommonPath(string[] paths)`

Finds the longest common path prefix among an array of paths.

- **Parameters**:
  - `paths`: The array of paths to compare. Must not be `null` and must contain at least one element.
- **Return value**: The longest common path prefix, or `null` if no common prefix exists or if any path is `null`.
- **Throws**: `ArgumentNullException` if `paths` is `null` or contains a `null` element.

---
### `public static string EnsureTrailingSeparator(string path)`

Ensures that the given path ends with a directory separator character.

- **Parameters**:
  - `path`: The path to process. Must not be `null`.
- **Return value**: The path with a trailing separator added if it did not already have one.
- **Throws**: `ArgumentNullException` if `path` is `null`.

---
### `public static string RemoveTrailingSeparator(string path)`

Removes the trailing directory separator character from the given path, if present.

- **Parameters**:
  - `path`: The path to process. Must not be `null`.
- **Return value**: The path with the trailing separator removed if it existed.
- **Throws**: `ArgumentNullException` if `path` is `null`.

## Usage
