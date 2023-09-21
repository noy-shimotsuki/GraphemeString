using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace TsukuyoOka.Text.Unicode;

[MemoryDiagnoser]
[ShortRunJob]
public partial class BenchmarkTest
{
    public static IEnumerable<string> LoadEmojiData()
    {
        foreach (var file in new[] { "emoji-test.txt", "emoji-sequences.txt", "emoji-zwj-sequences.txt" })
        {
            var text = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data", file));

            foreach (var part in new[] { Truncate5000().Match(text).Value, Truncate10000().Match(text).Value, text })
            {
                yield return part;
            }
        }
    }

    public static IEnumerable<object[]> LoadEvaluatedEmojiData()
    {
        foreach (var value in LoadEmojiData())
        {
            var graphemeString = new GraphemeString(value);
            _ = value.Length;
            foreach (var length in new[] { 10, 100, 1000, 2000 })
            {
                yield return new object[] { graphemeString, length };
            }
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(LoadEmojiData))]
    public int EvaluateGraphemes(string value)
    {
        return new GraphemeString(value).Length;
    }

    [Benchmark]
    [ArgumentsSource(nameof(LoadEmojiData))]
    public GraphemeString CreateInstance(string value)
    {
        return new GraphemeString(value);
    }

    [Benchmark]
    [ArgumentsSource(nameof(LoadEvaluatedEmojiData))]
    public GraphemeString TruncateByByteLength(GraphemeString value, int length)
    {
        return value.TruncateByByteLength(Encoding.UTF8, length);
    }

    [Benchmark]
    [ArgumentsSource(nameof(LoadEvaluatedEmojiData))]
    public GraphemeString TruncateByCodePointLength(GraphemeString value, int length)
    {
        return value.TruncateByCodePointLength(length);
    }

    [GeneratedRegex(@"^[\S\s]{5000,}?\n")]
    private static partial Regex Truncate5000();

    [GeneratedRegex(@"^[\S\s]{10000,}?\n")]
    private static partial Regex Truncate10000();
}