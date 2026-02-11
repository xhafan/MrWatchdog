using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace MrWatchdog.Core.Infrastructure.Localization;

public static class LocalizedTextResolver
{
    [return: NotNullIfNotNull(nameof(text))]
    public static string? ResolveLocalizedText(string? text, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        Dictionary<string, string>? map = null;
        try
        {
            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        var value = property.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            map[property.Name] = value;
                        }
                    }
                }

                if (map.Count == 0)
                {
                    map = null;
                }
            }
        }
        catch (JsonException)
        {
            map = null;
        }

        if (map == null)
        {
            return text;
        }

        if (map.TryGetValue(culture.TwoLetterISOLanguageName, out var val) && !string.IsNullOrWhiteSpace(val))
        {
            return val;
        }

        if (map.TryGetValue("en", out var enVal) && !string.IsNullOrWhiteSpace(enVal))
        {
            return enVal;
        }

        return map.Values.First();
    }    
}