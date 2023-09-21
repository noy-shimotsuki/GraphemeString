using System.Text.Json;
using System.Text.Json.Serialization;

namespace TsukuyoOka.Text.Unicode.Internals;

internal sealed class GraphemeStringJsonConverter : JsonConverter<GraphemeString>
{
    public override GraphemeString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.GetString() is { } baseString)
        {
            return baseString.Length > 0 ? new GraphemeString(baseString) : GraphemeString.Empty;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, GraphemeString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ValueSpan);
    }
}