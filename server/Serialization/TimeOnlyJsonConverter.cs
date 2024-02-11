using System.Formats.Asn1;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinigolfFriday.Serialization;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string TimeFormat = "HH:mm:ss.FFFFFFF";

    public override TimeOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var timeString = reader.GetString() ?? throw new JsonException();
        return TimeOnly.ParseExact(timeString, TimeFormat);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(TimeFormat));
    }
}
