using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using MrWatchdog.Core.Resources;

namespace MrWatchdog.Core.Infrastructure.Localization;

public static class TextWithResourceVariablesReplacer
{
    private static readonly Regex _resourceVariablePattern = new(@"\$\{Resource_(?<key>\w+)\}", RegexOptions.Compiled);

    [return: NotNullIfNotNull(nameof(text))]
    public static string? ReplaceResourceVariables(string? text, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return _resourceVariablePattern.Replace(text, match =>
        {
            var key = match.Groups["key"].Value;
            var localizedValue = Resource.ResourceManager.GetString(key, culture);
            return localizedValue ?? match.Value;
        });
    }    
}