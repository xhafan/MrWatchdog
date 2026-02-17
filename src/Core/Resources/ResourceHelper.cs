using System.Globalization;
using CoreUtils;

namespace MrWatchdog.Core.Resources;

public static class ResourceHelper
{
    public static string GetString(string resourceName, CultureInfo culture)
    {
        var translation = Resource.ResourceManager.GetString(resourceName, culture);
        Guard.Hope(translation != null, $"No {culture} translation for {resourceName}.");
        return translation;
    }
}