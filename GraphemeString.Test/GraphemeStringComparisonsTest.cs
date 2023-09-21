using System;
using System.Reflection;
using System.Text.Json;
using TsukuyoOka.Text.Unicode.Utils;

namespace TsukuyoOka.Text.Unicode;

public class GraphemeStringComparisonsTest
{
    public static IEnumerable<object[]> LoadData(string categoryName, string functionName)
    {
        using var stream = File.OpenRead(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data", "test-pattern.json"));
        var json = JsonSerializer.Deserialize<JsonElement>(stream);
        var category = json.GetProperty(categoryName);
        var testValues = category.GetProperty("values").EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var (data, value) in category.GetProperty(functionName).EnumerateArray().Zip(testValues))
        {
            foreach (var item in data.EnumerateArray())
            {
                yield return new object[]
                {
                    value,
                    item[0].GetString()!,
                    item[1].ValueKind switch
                    {
                        JsonValueKind.True or JsonValueKind.False => item[1].GetBoolean(),
                        JsonValueKind.Number => item[1].GetInt32(),
                        _ => item[1].GetString()!
                    }
                };
            }
        }
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑__", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🤝\u200D🧑__", StringComparison.Ordinal, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑__", StringComparison.InvariantCulture, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🤝\u200D🧑__", StringComparison.InvariantCulture, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++\U0001f9d1‍\U0001f91d‍\U0001f9d1--\U0001f9d1‍\U0001f91d‍\U0001f9d1__", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "\U0001f91d‍\U0001f9d1--\U0001f9d1‍\U0001f91d‍\U0001f9d1__", StringComparison.Ordinal, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", StringComparison.Ordinal, true)]
    [InlineData("さんか\u3099つ", "\u3099つ", StringComparison.Ordinal, false)]
    [InlineData("さんか\u3099つ", "\u3099つ", StringComparison.InvariantCulture, false)]
    [InlineData("さんか\u3099つ", "さんか\u3099つ", StringComparison.Ordinal, true)]
    [InlineData("さんか\u3099つ", "さんか\u3099つ", StringComparison.InvariantCulture, true)]
    [InlineData("さんか\u3099つ", "さんがつ", StringComparison.Ordinal, false)]
    [InlineData("さんか\u3099つ", "さんがつ", StringComparison.InvariantCulture, true)]
    [InlineData("さんかつ", "さんがつ", StringComparison.InvariantCulture, false)]
    [InlineData("", "aaa", StringComparison.Ordinal, false)]
    [InlineData("aaa", "", StringComparison.Ordinal, true)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "DEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", StringComparison.Ordinal, true)]
    [InlineData("AAA\r\n", "\r", StringComparison.Ordinal, false)]
    [InlineData("AAA\r\n", "\r", StringComparison.OrdinalIgnoreCase, false)]
    [InlineData("AAA\r\n", "\r", StringComparison.InvariantCulture, false)]
    [InlineData("AAA\r\n", "\n", StringComparison.Ordinal, false)]
    [InlineData("AAA\r\n", "\n", StringComparison.OrdinalIgnoreCase, false)]
    [InlineData("AAA\r\n", "\n", StringComparison.InvariantCulture, true)]
    [InlineData("AAA", "\r", StringComparison.Ordinal, false)]
    [InlineData("AAA", "\n", StringComparison.Ordinal, false)]
    public void TestEndsWith(string testString, string value, StringComparison stringComparison, bool expected)
    {
        var str = new GraphemeString(testString);
        Assert.Equal(expected, str.EndsWith(value, stringComparison));
        Assert.Equal(expected, str.EndsWith(new GraphemeString(value), stringComparison));
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 2)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "+🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "-🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 4)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🤝", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "--", StringComparison.Ordinal, 3)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "__", StringComparison.Ordinal, 6)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 2)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.Ordinal, -1)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.InvariantCulture, -1)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.Ordinal, 0)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.Ordinal, -1)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "つ", StringComparison.Ordinal, 3)]
    [InlineData("", "aaa", StringComparison.Ordinal, -1)]
    [InlineData("aaa", "", StringComparison.Ordinal, 0)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "DEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", StringComparison.Ordinal, 3)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.Ordinal, -1)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.OrdinalIgnoreCase, -1)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.InvariantCulture, 3)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.Ordinal, -1)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.OrdinalIgnoreCase, -1)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.InvariantCulture, 3)]
    [InlineData("AAA", "\r", StringComparison.Ordinal, -1)]
    [InlineData("AAA", "\n", StringComparison.Ordinal, -1)]
    public void TestIndexOf(string testString, string value, StringComparison stringComparison, int expected)
    {
        var str = new GraphemeString(testString);
        Assert.Equal(expected, str.IndexOf(value, stringComparison));
        Assert.Equal(expected, str.IndexOf(new GraphemeString(value), stringComparison));
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 5)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "+🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "-🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 4)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🤝", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "--", StringComparison.Ordinal, 3)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "__", StringComparison.Ordinal, 6)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 2)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.Ordinal, -1)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.InvariantCulture, -1)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.Ordinal, 0)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.Ordinal, -1)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "つ", StringComparison.Ordinal, 3)]
    [InlineData("", "aaa", StringComparison.Ordinal, -1)]
    [InlineData("aaa", "", StringComparison.Ordinal, 3)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "DEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", StringComparison.Ordinal, 3)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.Ordinal, -1)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.OrdinalIgnoreCase, -1)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.InvariantCulture, 3)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.Ordinal, -1)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.OrdinalIgnoreCase, -1)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.InvariantCulture, 3)]
    [InlineData("AAA", "\r", StringComparison.Ordinal, -1)]
    [InlineData("AAA", "\n", StringComparison.Ordinal, -1)]
    public void TestLastIndexOf(string testString, string value, StringComparison stringComparison, int expected)
    {
        var str = new GraphemeString(testString);
        Assert.Equal(expected, str.LastIndexOf(value, stringComparison));
        Assert.Equal(expected, str.LastIndexOf(new GraphemeString(value), stringComparison));
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝", StringComparison.Ordinal, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑", StringComparison.InvariantCulture, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝", StringComparison.InvariantCulture, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++\U0001f9d1‍\U0001f91d‍\U0001f9d1--\U0001f9d1‍\U0001f91d‍\U0001f9d1__", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++\U0001f9d1‍\U0001f91d‍\U0001f9d1--\U0001f9d1‍\U0001f91d", StringComparison.Ordinal, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, true)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.Ordinal, false)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.InvariantCulture, false)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.Ordinal, true)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.InvariantCulture, true)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.Ordinal, false)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.InvariantCulture, true)]
    [InlineData("さんかつ", "さんがつ", StringComparison.InvariantCulture, false)]
    [InlineData("", "aaa", StringComparison.Ordinal, false)]
    [InlineData("aaa", "", StringComparison.Ordinal, true)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "DEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", StringComparison.Ordinal, false)]
    [InlineData("\r\nAAA", "\r", StringComparison.Ordinal, false)]
    [InlineData("\r\nAAA", "\r", StringComparison.OrdinalIgnoreCase, false)]
    [InlineData("\r\nAAA", "\r", StringComparison.InvariantCulture, true)]
    [InlineData("\r\nAAA", "\n", StringComparison.Ordinal, false)]
    [InlineData("\r\nAAA", "\n", StringComparison.OrdinalIgnoreCase, false)]
    [InlineData("\r\nAAA", "\n", StringComparison.InvariantCulture, false)]
    [InlineData("AAA", "\r", StringComparison.Ordinal, false)]
    [InlineData("AAA", "\n", StringComparison.Ordinal, false)]
    public void TestStartsWith(string testString, string value, StringComparison stringComparison, bool expected)
    {
        var str = new GraphemeString(testString);
        Assert.Equal(expected, str.StartsWith(value, stringComparison));
        Assert.Equal(expected, str.StartsWith(new GraphemeString(value), stringComparison));
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 2)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "+🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "-🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 11)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🤝", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "--", StringComparison.Ordinal, 10)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "__", StringComparison.Ordinal, 20)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 2)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.Ordinal, -1)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.InvariantCulture, -1)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.Ordinal, 0)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.Ordinal, -1)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "つ", StringComparison.Ordinal, 4)]
    [InlineData("", "aaa", StringComparison.Ordinal, -1)]
    [InlineData("aaa", "", StringComparison.Ordinal, 0)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "DEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", StringComparison.Ordinal, 3)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.Ordinal, -1)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.OrdinalIgnoreCase, -1)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.InvariantCulture, 3)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.Ordinal, -1)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.OrdinalIgnoreCase, -1)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.InvariantCulture, 4)]
    [InlineData("AAA", "\r", StringComparison.Ordinal, -1)]
    [InlineData("AAA", "\n", StringComparison.Ordinal, -1)]
    public void TestCharIndexOf(string testString, string value, StringComparison stringComparison, int expected)
    {
        var str = new GraphemeString(testString);
        Assert.Equal(expected, str.CharIndexOf(value, stringComparison));
        Assert.Equal(expected, str.CharIndexOf(new GraphemeString(value), stringComparison));
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 12)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "+🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "-🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 11)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🤝", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "--", StringComparison.Ordinal, 10)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "__", StringComparison.Ordinal, 20)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, 2)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.Ordinal, -1)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.InvariantCulture, -1)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.Ordinal, 0)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.Ordinal, -1)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "つ", StringComparison.Ordinal, 4)]
    [InlineData("", "aaa", StringComparison.Ordinal, -1)]
    [InlineData("aaa", "", StringComparison.Ordinal, 3)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "DEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", StringComparison.Ordinal, 3)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.Ordinal, -1)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.OrdinalIgnoreCase, -1)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.InvariantCulture, 4)]
    [InlineData("AAA", "\r", StringComparison.Ordinal, -1)]
    [InlineData("AAA", "\n", StringComparison.Ordinal, -1)]
    public void TestLastCharIndexOf(string testString, string value, StringComparison stringComparison, int expected)
    {
        var str = new GraphemeString(testString);
        Assert.Equal(expected, str.LastCharIndexOf(value, stringComparison));
        Assert.Equal(expected, str.LastCharIndexOf(new GraphemeString(value), stringComparison));
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "+🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "-🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑", StringComparison.Ordinal, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🤝", StringComparison.Ordinal, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝", StringComparison.Ordinal, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "--", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "__", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑", StringComparison.Ordinal, true)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.Ordinal, false)]
    [InlineData("さんか\u3099つ", "さんか", StringComparison.InvariantCulture, false)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.Ordinal, true)]
    [InlineData("さんか\u3099つ", "さんか\u3099", StringComparison.InvariantCulture, true)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.Ordinal, false)]
    [InlineData("さんか\u3099つ", "さんが", StringComparison.InvariantCulture, true)]
    [InlineData("さんか\u3099つ", "つ", StringComparison.Ordinal, true)]
    [InlineData("", "aaa", StringComparison.Ordinal, false)]
    [InlineData("aaa", "", StringComparison.Ordinal, true)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "DEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", StringComparison.Ordinal, true)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.Ordinal, false)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.OrdinalIgnoreCase, false)]
    [InlineData("AAA\r\nBBB", "\r", StringComparison.InvariantCulture, true)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.Ordinal, false)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.OrdinalIgnoreCase, false)]
    [InlineData("AAA\r\nBBB", "\n", StringComparison.InvariantCulture, true)]
    [InlineData("AAA", "\r", StringComparison.Ordinal, false)]
    [InlineData("AAA", "\n", StringComparison.Ordinal, false)]
    public void TestContains(string testString, string value, StringComparison stringComparison, bool expected)
    {
        var str = new GraphemeString(testString);
        Assert.Equal(expected, str.Contains(value, stringComparison));
        Assert.Equal(expected, str.Contains(new GraphemeString(value), stringComparison));
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", StringComparison.Ordinal, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑🤝🧑__", StringComparison.Ordinal, false)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", StringComparison.InvariantCulture, true)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑🤝🧑__", StringComparison.InvariantCulture, true)]
    [InlineData("さんか\u3099つ", "さんかつ", StringComparison.Ordinal, false)]
    [InlineData("さんか\u3099つ", "さんかつ", StringComparison.InvariantCulture, false)]
    [InlineData("さんか\u3099つ", "さんか\u3099つ", StringComparison.Ordinal, true)]
    [InlineData("さんか\u3099つ", "さんか\u3099つ", StringComparison.InvariantCulture, true)]
    [InlineData("さんか\u3099つ", "さんがつ", StringComparison.Ordinal, false)]
    [InlineData("さんか\u3099つ", "さんがつ", StringComparison.InvariantCulture, true)]
    public void TestEquals(string testString, string value, StringComparison stringComparison, bool expected)
    {
        var str = new GraphemeString(testString);
        Assert.Equal(expected, str.Equals(value, stringComparison));
        Assert.Equal(expected, str.Equals(new GraphemeString(value), stringComparison));
    }

    [Fact]
    public void TestOverriddenEquals()
    {
        var str = new GraphemeString("test");
        Assert.False(str.Equals(null));
        Assert.False(str.Equals("test"));
        Assert.True(str.Equals(new GraphemeString("test")));
    }

    [Fact]
    public void TestGetHashCode()
    {
        Assert.True(new GraphemeString("test").GetHashCode() == new GraphemeString("test").GetHashCode());
        Assert.False(new GraphemeString("test").GetHashCode() == new GraphemeString("test2").GetHashCode());
        Assert.True(new GraphemeString("testtesttesttesttest").GetHashCode() == new GraphemeString("testtesttesttesttest").GetHashCode());
        Assert.False(new GraphemeString("testtesttesttesttest").GetHashCode() == new GraphemeString("testtesttesttesttest2").GetHashCode());
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", StringComparison.Ordinal, 0)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑🤝🧑__", StringComparison.Ordinal, -1)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", StringComparison.InvariantCulture, 0)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑🤝🧑__", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "さんかつ", StringComparison.Ordinal, 1)]
    [InlineData("さんか\u3099つ", "さんかつ", StringComparison.InvariantCulture, 1)]
    [InlineData("さんか\u3099つ", "さんか\u3099つ", StringComparison.Ordinal, 0)]
    [InlineData("さんか\u3099つ", "さんか\u3099つ", StringComparison.InvariantCulture, 0)]
    [InlineData("さんか\u3099つ", "さんがつ", StringComparison.Ordinal, -1)]
    [InlineData("さんか\u3099つ", "さんがつ", StringComparison.InvariantCulture, 0)]
    public void TestCompareTo(string testString, string value, StringComparison stringComparison, int expected)
    {
        var str = new GraphemeString(testString);
        switch (expected)
        {
            case 0:
                Assert.Equal(0, str.CompareTo(value, stringComparison));
                Assert.Equal(0, str.CompareTo(new GraphemeString(value), stringComparison));
                break;
            case < 0:
                AssertUtil.LowerThan(0, str.CompareTo(value, stringComparison));
                AssertUtil.LowerThan(0, str.CompareTo(new GraphemeString(value), stringComparison));
                break;
            default:
                AssertUtil.GreaterThan(0, str.CompareTo(value, stringComparison));
                AssertUtil.GreaterThan(0, str.CompareTo(new GraphemeString(value), stringComparison));
                break;
        }
    }

    [Theory]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", 0)]
    [InlineData("++🧑\u200D🤝\u200D🧑--🧑\u200D🤝\u200D🧑__", "++🧑\u200D🤝\u200D🧑--🧑🤝🧑__", -1)]
    [InlineData("さんか\u3099つ", "さんかつ", 1)]
    public void TestIComparableCompareTo(string testString, string value, int expected)
    {
        var str = new GraphemeString(testString);
        switch (expected)
        {
            case 0:
                Assert.Equal(0, ((IComparable<GraphemeString>)str).CompareTo(new GraphemeString(value)));
                break;
            case < 0:
                AssertUtil.LowerThan(0, ((IComparable<GraphemeString>)str).CompareTo(new GraphemeString(value)));
                break;
            default:
                AssertUtil.GreaterThan(0, ((IComparable<GraphemeString>)str).CompareTo(new GraphemeString(value)));
                break;
        }
    }
}