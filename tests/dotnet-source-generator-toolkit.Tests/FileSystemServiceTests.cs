using System;
using System.IO;
using System.Threading.Tasks;
using DotNetSourceGeneratorToolkit.Exceptions;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests
{
	/// <summary>
	/// Contains unit tests for the <see cref="FileSystemService"/> class.
	/// Tests file system operations including reading, writing, creating directories,
	/// checking file existence, deleting files, and appending content.
	/// </summary>
	public class FileSystemServiceTests
	{
		/// <summary>
		/// Creates a new instance of <see cref="FileSystemService"/> with a null logger.
		/// </summary>
		/// <returns>A new <see cref="FileSystemService"/> instance.</returns>
		private FileSystemService CreateService()
		{
			return new FileSystemService(NullLogger<FileSystemService>.Instance);
		}

		[Fact]
		/// <summary>
		/// Tests that writing and reading a file returns the same content.
		/// Verifies the roundtrip operation preserves the original content.
		/// </summary>
		public async Task WriteReadRoundtrip_ShouldReturnSameContent()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempDir);
			var filePath = Path.Combine(tempDir, "test.txt");
			var content = "Hello, world!";

			try
			{
				var service = CreateService();

				await service.WriteFileAsync(filePath, content);
				var readContent = await service.ReadFileAsync(filePath);

				Assert.Equal(content, readContent);
			}
			finally
			{
				if (Directory.Exists(tempDir))
				{
					Directory.Delete(tempDir, true);
				}
			}
		}

		[Fact]
		/// <summary>
		/// Tests that reading a non-existent file throws a <see cref="FileSystemException"/>.
		/// Verifies proper error handling when attempting to read missing files.
		/// </summary>
		public async Task ReadFileAsync_MissingFile_ShouldThrowFileSystemException()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempDir);
			var missingFile = Path.Combine(tempDir, "missing.txt");

			try
			{
				var service = CreateService();

				await Assert.ThrowsAsync<FileSystemException>(async () =>
					await service.ReadFileAsync(missingFile));
			}
			finally
			{
				if (Directory.Exists(tempDir))
				{
					Directory.Delete(tempDir, true);
				}
			}
		}

		[Fact]
		/// <summary>
		/// Tests that creating nested directories creates all intermediate directories.
		/// Verifies that the service can create directory structures recursively.
		/// </summary>
		public async Task CreateDirectoryAsync_NestedDirectories_ShouldCreateAll()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempDir);
			var nestedDir = Path.Combine(tempDir, "a", "b", "c");

			try
			{
				var service = CreateService();

				await service.CreateDirectoryAsync(nestedDir);

				Assert.True(Directory.Exists(nestedDir));
			}
			finally
			{
				if (Directory.Exists(tempDir))
				{
					Directory.Delete(tempDir, true);
				}
			}
		}

		[Fact]
		/// <summary>
		/// Tests file existence checking and deletion operations.
		/// Verifies that FileExists returns correct state before and after deletion.
		/// </summary>
		public async Task FileExistsAndDeleteFileAsync_ShouldReflectFileState()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempDir);
			var filePath = Path.Combine(tempDir, "toDelete.txt");

			try
			{
				var service = CreateService();

				await service.WriteFileAsync(filePath, "delete me");
				Assert.True(service.FileExists(filePath));

				await service.DeleteFileAsync(filePath);
				Assert.False(service.FileExists(filePath));
			}
			finally
			{
				if (Directory.Exists(tempDir))
				{
					Directory.Delete(tempDir, true);
				}
			}
		}

		[Fact]
		/// <summary>
		/// Tests appending content to an existing file.
		/// Verifies that content is correctly appended without overwriting existing content.
		/// </summary>
		public async Task AppendFileAsync_ShouldAddContent()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempDir);
			var filePath = Path.Combine(tempDir, "append.txt");
			var initial = "first line\n";
			var appended = "second line\n";

			try
			{
				var service = CreateService();

				await service.WriteFileAsync(filePath, initial);
				await service.AppendFileAsync(filePath, appended);

				var content = await service.ReadFileAsync(filePath);
				Assert.Equal(initial + appended, content);
			}
			finally
			{
				if (Directory.Exists(tempDir))
				{
					Directory.Delete(tempDir, true);
				}
			}
		}
	}
}
