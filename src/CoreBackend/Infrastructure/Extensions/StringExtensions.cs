using System.Globalization;
using System.Text;

namespace MrWatchdog.Core.Infrastructure.Extensions;

public static class StringExtensions
{
    // C# version of includesIgnoringDiacritics() TS function
    public static bool ContainsIgnoringDiacritics(this string text, string substring)
    {
        return _normalize(text).Contains(_normalize(substring));
        
        string _normalize(string str)
        {
            return string.Concat(str
                        .Normalize(NormalizationForm.FormD) // Decompose accents
                        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) // Remove accents
                )
                .ToLowerInvariant();
        }        
    }
}