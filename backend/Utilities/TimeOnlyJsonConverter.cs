
using System.Text.Json;
using System.Text.Json.Serialization;


namespace backend.Utilities;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string TimeFormat = "HH:mm:ss";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var timeString = reader.GetString();
        if (TimeOnly.TryParseExact(timeString, TimeFormat, out var time))
        {
            return time;
        }
        throw new FormatException($"String '{timeString}' was not recognized as a valid TimeOnly in format '{TimeFormat}'.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(TimeFormat));
    }
}