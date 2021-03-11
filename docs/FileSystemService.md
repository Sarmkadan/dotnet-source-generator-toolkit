# FileSystemService
The `FileSystemService` class provides a set of methods for interacting with the file system, allowing for reading, writing, appending, deleting, and checking the existence of files, as well as creating directories and retrieving file names.

## API
* `public FileSystemService`: The constructor for the `FileSystemService` class.
* `public async Task<string> ReadFileAsync`: Reads the contents of a file asynchronously. Returns the contents of the file as a string. Throws if the file does not exist or an error occurs while reading.
* `public async Task WriteFileAsync`: Writes to a file asynchronously. Throws if an error occurs while writing.
* `public async Task AppendFileAsync`: Appends to a file asynchronously. Throws if an error occurs while appending.
* `public bool FileExists`: Checks if a file exists. Returns `true` if the file exists, `false` otherwise.
* `public async Task DeleteFileAsync`: Deletes a file asynchronously. Throws if the file does not exist or an error occurs while deleting.
* `public async Task CreateDirectoryAsync`: Creates a directory asynchronously. Throws if an error occurs while creating the directory.
* `public async Task<IEnumerable<string>> GetFilesAsync`: Retrieves a list of files in a directory asynchronously. Returns a list of file names as strings. Throws if an error occurs while retrieving the list of files.
* `public string GetDirectoryName`: Retrieves the directory name from a file path. Returns the directory name as a string.
* `public string CombinePath`: Combines two or more path segments into a single path. Returns the combined path as a string.

## Usage
```csharp
// Example 1: Reading and writing a file
var fileSystemService = new FileSystemService();
var filePath = "example.txt";
var fileContents = "Hello, world!";
await fileSystemService.WriteFileAsync(filePath, fileContents);
var readContents = await fileSystemService.ReadFileAsync(filePath);
Console.WriteLine(readContents); // Outputs: Hello, world!

// Example 2: Creating a directory and listing its files
var directoryPath = "exampleDir";
await fileSystemService.CreateDirectoryAsync(directoryPath);
var files = await fileSystemService.GetFilesAsync(directoryPath);
foreach (var file in files)
{
    Console.WriteLine(file);
}
```

## Notes
The `FileSystemService` class is designed to be used in a variety of scenarios, including file I/O operations and directory management. However, it is worth noting that some methods may throw exceptions if the underlying file system operations fail. Additionally, the class is not thread-safe, and concurrent access to its methods may result in unexpected behavior. When using the `FileSystemService` class, it is recommended to handle exceptions and ensure that file system operations are properly synchronized to avoid conflicts.
