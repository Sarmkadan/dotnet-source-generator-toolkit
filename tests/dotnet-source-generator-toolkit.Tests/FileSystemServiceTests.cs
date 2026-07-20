using System;
using System.IO;
using System.Threading.Tasks;
using DotNetSourceGeneratorToolkit.Exceptions;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests
{
    public class FileSystemServiceTests
    {
        private FileSystemService CreateService()
        {
            return new FileSystemService(NullLogger<FileSystemService>.Instance);
        }

        [Fact]
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
