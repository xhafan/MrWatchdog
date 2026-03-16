using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MrWatchdog.Core.Infrastructure.Jsons;

public class CultureInfoJsonConverter : JsonConverter<CultureInfo>
{
    public override CultureInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        var name = reader.GetString();
        return name is null ? null : new CultureInfo(name);
    }

    public override void Write(Utf8JsonWriter writer, CultureInfo? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.Name);
    }
}