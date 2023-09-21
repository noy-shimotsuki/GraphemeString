using System.Text.Json;

namespace TsukuyoOka.Text.Unicode;

public class GraphemeStringSerializeTest
{
    [Fact]
    public void JsonSerializeTest()
    {
        Assert.Equal("\"test string\"", JsonSerializer.Serialize(new GraphemeString("test string")));
        Assert.Equal("{\"Value\":\"test string\"}", JsonSerializer.Serialize(new { Value = new GraphemeString("AAA test string AAA", 4..^4) }));
    }

    [Fact]
    public void JsonDeserializeTest()
    {
        Assert.Null(JsonSerializer.Deserialize<GraphemeString>("null"));
        Assert.Equal(new GraphemeString("test string"), JsonSerializer.Deserialize<GraphemeString>("\"test string\""));
        Assert.Same(GraphemeString.Empty, JsonSerializer.Deserialize<GraphemeString>("\"\""));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<GraphemeString>("0"));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<GraphemeString>("true"));
    }
}