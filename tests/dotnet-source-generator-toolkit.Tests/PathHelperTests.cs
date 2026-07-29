using System;
using System.IO;
using DotNetSourceGeneratorToolkit.Utilities;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests
{
    public class PathHelperTests
    {
        [Fact]
        public void NormalizePath_RemovesTrailingSeparator()
        {
            // Arrange
            var pathWithSeparator = Path.Combine("C:", "Temp") + Path.DirectorySeparatorChar;
            var expected = Path.GetFullPath(Path.Combine("C:", "Temp"));

            // Act
            var result = PathHelper.NormalizePath(pathWithSeparator);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void NormalizePath_HandlesForwardSlashOnWindows()
        {
            // Arrange
            var forwardSlashPath = "C:/Temp/Folder/";
            var expected = Path.GetFullPath("C:\\Temp\\Folder");

            // Act
            var result = PathHelper.NormalizePath(forwardSlashPath);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToRelativePath_ReturnsCorrectRelativePath()
        {
            // Arrange
            var basePath = Path.GetFullPath(Path.Combine("C:", "Projects"));
            var absolutePath = Path.GetFullPath(Path.Combine("C:", "Projects", "Solution", "Project.csproj"));
            var expected = Path.GetRelativePath(basePath, absolutePath);

            // Act
            var result = PathHelper.ToRelativePath(absolutePath, basePath);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsAbsolute_ReturnsTrueForAbsolutePath()
        {
            // Arrange
            var absolutePath = Path.GetFullPath("C:\\Temp");

            // Act
            var result = PathHelper.IsAbsolute(absolutePath);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsAbsolute_ReturnsFalseForRelativePath()
        {
            // Arrange
            var relativePath = "Temp\\Folder";

            // Act
            var result = PathHelper.IsAbsolute(relativePath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetCommonPath_ReturnsCommonRoot()
        {
            // Arrange
            var path1 = Path.GetFullPath(Path.Combine("C:", "Projects", "SolutionA"));
            var path2 = Path.GetFullPath(Path.Combine("C:", "Projects", "SolutionB"));
            var expected = Path.GetFullPath(Path.Combine("C:", "Projects"));

            // Act
            var result = PathHelper.GetCommonPath(path1, path2);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EnsureTrailingSeparator_AddsSeparatorIfMissing()
        {
            // Arrange
            var path = Path.GetFullPath(Path.Combine("C:", "Temp"));
            var expected = path + Path.DirectorySeparatorChar;

            // Act
            var result = PathHelper.EnsureTrailingSeparator(path);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EnsureTrailingSeparator_DoesNotDuplicateSeparator()
        {
            // Arrange
            var path = Path.GetFullPath(Path.Combine("C:", "Temp")) + Path.DirectorySeparatorChar;
            var expected = path;

            // Act
            var result = PathHelper.EnsureTrailingSeparator(path);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void RemoveTrailingSeparator_RemovesSeparator()
        {
            // Arrange
            var path = Path.GetFullPath(Path.Combine("C:", "Temp")) + Path.DirectorySeparatorChar;
            var expected = Path.GetFullPath(Path.Combine("C:", "Temp"));

            // Act
            var result = PathHelper.RemoveTrailingSeparator(path);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void RemoveTrailingSeparator_DoesNotAffectNonTrailing()
        {
            // Arrange
            var path = Path.GetFullPath(Path.Combine("C:", "Temp"));
            var expected = path;

            // Act
            var result = PathHelper.RemoveTrailingSeparator(path);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
