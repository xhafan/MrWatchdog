using System.Text.Json;

namespace MrWatchdog.Core.Infrastructure;

public static class JsonHelper
{
    public static string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, JsonSerializerOptions.Web);
    }

    public static TValue? Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, JsonSerializerOptions.Web);
    }
}