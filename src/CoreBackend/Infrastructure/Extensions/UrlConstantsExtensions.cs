using CoreUtils;

namespace MrWatchdog.Core.Features;

public static class UrlConstantsExtensions
{
    public static string WithVariable(this string urlTemplate, string variableName, string variableValue)
    {
        Guard.Hope(urlTemplate.Contains(variableName), $"{urlTemplate} does not contain variable {variableName}");
        return urlTemplate.Replace(variableName, variableValue);
    }
}