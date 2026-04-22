using System.Text.Json;

namespace CoreBackend.Infrastructure.Jsons;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var opts = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opts.Converters.Add(new CultureInfoJsonConverter());
        return opts;
    }

    public static string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, DefaultOptions);
    }

    public static TValue? Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, DefaultOptions);
    }
}