using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.Domain
{
    public static class GenerationTemplateExtensions
    {
        public static bool IsValid(this GenerationTemplate template)
        {
            return !template.Validate().Any();
        }

        public static string GetDisplayName(this GenerationTemplate template)
        {
            return $"{template.Name} ({template.GeneratorType})";
        }

        public static bool IsSupportedLanguage(this GenerationTemplate template, string language)
        {
            return template.SupportedLanguages.Contains(language, StringComparer.OrdinalIgnoreCase);
        }

        public static IEnumerable<string> GetConfigurationOptionKeys(this GenerationTemplate template)
        {
            return template.ConfigurationOptions.Keys;
        }
    }
}
