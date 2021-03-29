using System;

namespace DotNetSourceGeneratorToolkit.Exceptions
{
    public static class GenerationExceptionExtensions
    {
        public static bool IsGeneratorType(this GenerationException exception, string generatorType) 
            => exception.GeneratorType?.Equals(generatorType, StringComparison.OrdinalIgnoreCase) ?? false;

        public static string GetErrorMessageWithDetails(this GenerationException exception) 
            => $"{exception.Message} (GeneratorType: {exception.GeneratorType}, EntityName: {exception.EntityName})";

        public static bool HasEntityName(this GenerationException exception) 
            => !string.IsNullOrWhiteSpace(exception.EntityName);
    }
}
