﻿using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TsukuyoOka.Text.Unicode;

public class GraphemeStringBasicTest
{
    [Theory]
    [MemberData(nameof(LoadData), "emoji-test.txt", @"#(?:(?<Grapheme>\x20\uD83C[\uDFFB-\uDFFF])|\x20(?<Grapheme>[^\x20]+))\x20")]
    public void TestUnicode14Emoji1(int charLength, int length, (int index, string value)[] graphemes, string line)
    {
        var str = new GraphemeString(line);
        Assert.Equal(charLength, str.CharLength);
        Assert.Equal(length, str.Length);
        foreach (var (index, value) in graphemes)
        {
            Assert.Equal(value, str[index].ToString());
        }
    }

    [Theory]
    [MemberData(nameof(LoadData), "emoji-sequences.txt", @"#.+\((?<Grapheme>.+?)\)")]
    public void TestUnicode14Emoji2(int charLength, int length, (int index, string value)[] graphemes, string line)
    {
        var str = new GraphemeString(line);
        Assert.Equal(charLength, str.CharLength);
        Assert.Equal(length, str.Length);
        foreach (var (index, value) in graphemes)
        {
            Assert.Equal(value, str[index].ToString());
        }
    }

    [Theory]
    [MemberData(nameof(LoadData), "emoji-zwj-sequences.txt", @"#.+\((?<Grapheme>.+?)\)")]
    public void TestUnicode14Emoji3(int charLength, int length, (int index, string value)[] graphemes, string line)
    {
        var str = new GraphemeString(line);
        Assert.Equal(charLength, str.CharLength);
        Assert.Equal(length, str.Length);
        foreach (var (index, value) in graphemes)
        {
            Assert.Equal(value, str[index].ToString());
        }
    }

    [Theory]
    [InlineData("", 0, 0, "")]
    [InlineData("あいうえおがぎぐげごぱぴぷぺぽ", 3, 10, "えおがぎぐげごぱぴぷ")]
    [InlineData("あいうえおか\u3099き\u3099く\u3099け\u3099こ\u3099は\u309aひ\u309aふ\u309aへ\u309aほ\u309a", 3, 10, "えおか\u3099き\u3099く\u3099け\u3099")]
    [InlineData("👨\u200d👩\u200d👧\u200d👦", 3, 5, "👩\u200d👧")]
    [InlineData("👨👩👧👦", 2, 4, "👩👧")]
    [InlineData("🇺🇸🇰🇷🇺🇸🇰🇷", 6, 4, "🇷🇺")]
    [InlineData("🏴\U000E0067\U000E0062\U000E0065\U000E006E\U000E0067\U000E007F", 4, 4, "\U000E0062\U000E0065")]
    [InlineData("👩\U0001F3FD\u200d❤\uFE0F\u200d💋\u200d👨\U0001F3FB", 5, 2, "❤\uFE0F")]
    [InlineData("👩\U0001F3FD❤\uFE0F💋👨\U0001F3FB", 8, 2, "👨")]
    public void TestConstructor(string testValue, int start, int length, string expectedValue)
    {
        Assert.Equal(testValue, testValue.ToGraphemeString().ToString());
        Assert.Equal(testValue, new GraphemeString(testValue).ToString());
        Assert.Equal(expectedValue, new GraphemeString(testValue, start, length).ToString());
        Assert.Equal(expectedValue, new GraphemeString(testValue, start..(start + length)).ToString());
        Assert.Equal(expectedValue, new GraphemeString(testValue, start..^(testValue.Length - start - length)).ToString());
    }

    [Fact]
    public void TestConstructorException()
    {
        var testValue = "あいうえお";
        Assert.Throws<ArgumentOutOfRangeException>(() => new GraphemeString(testValue, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GraphemeString(testValue, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GraphemeString(testValue, testValue.Length + 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GraphemeString(testValue, 1, testValue.Length));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GraphemeString(testValue, 0, testValue.Length + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GraphemeString(testValue, -1..));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GraphemeString(testValue, ..-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GraphemeString(testValue, ..(testValue.Length + 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GraphemeString(testValue, 1..(testValue.Length + 1)));
    }

    [Theory]
    [InlineData("", 0, 0)]
    [InlineData("あいうえおがぎぐげごぱぴぷぺぽ", 15, 15)]
    [InlineData("あいうえおか\u3099き\u3099く\u3099け\u3099こ\u3099は\u309aひ\u309aふ\u309aへ\u309aほ\u309a", 25, 15)]
    [InlineData("👨\u200d👩\u200d👧\u200d👦", 11, 1)]
    [InlineData("👨👩👧👦", 8, 4)]
    [InlineData("🇺🇸🇰🇷🇺🇸🇰🇷", 16, 4)]
    [InlineData("🏴\U000E0067\U000E0062\U000E0065\U000E006E\U000E0067\U000E007F", 14, 1)]
    [InlineData("👩\U0001F3FD\u200d❤\uFE0F\u200d💋\u200d👨\U0001F3FB", 15, 1)]
    [InlineData("👩\U0001F3FD❤\uFE0F💋👨\U0001F3FB", 12, 4)]
    public void TestLength(string testValue, int expectedCharLength, int expectedLength)
    {
        var str = new GraphemeString(testValue);
        Assert.Equal(expectedCharLength, str.CharLength);
        Assert.Equal(expectedLength, str.Length);
        Assert.Equal(expectedCharLength is 0, str.IsEmpty);
        Assert.Equal(expectedLength, testValue.CountGraphemes());
    }

    [Theory]
    [InlineData("あいうえおがぎぐげごぱぴぷぺぽ", 5, 10, "が", 7, 10, 8, 5, "ぐげご")]
    [InlineData("あいうえおか\u3099き\u3099く\u3099け\u3099こ\u3099は\u309aひ\u309aふ\u309aへ\u309aほ\u309a",
        5, 10, "か\u3099", 7, 10, 8, 5, "く\u3099け\u3099こ\u3099")]
    [InlineData("👨\u200d👩\u200d👧\u200d👦", 0, 1, "👨\u200d👩\u200d👧\u200d👦", 0, 1, 1, 0, "👨\u200d👩\u200d👧\u200d👦")]
    [InlineData("👨👩👧👦", 1, 3, "👩", 1, 3, 3, 1, "👩👧")]
    [InlineData("🇺🇸🇰🇷🇺🇸🇰🇷", 2, 2, "🇺🇸", 2, 4, 2, 0, "🇺🇸🇰🇷")]
    [InlineData("🏴\U000E0067\U000E0062\U000E0065\U000E006E\U000E0067\U000E007F",
        0, 1, "🏴\U000E0067\U000E0062\U000E0065\U000E006E\U000E0067\U000E007F", 1, 1, 0, 0, "")]
    [InlineData("👩\U0001F3FD\u200d❤\uFE0F\u200d💋\u200d👨\U0001F3FB",
        0, 1, "👩\U0001F3FD\u200d❤\uFE0F\u200d💋\u200d👨\U0001F3FB", 0, 1, 1, 0, "👩\U0001F3FD\u200d❤\uFE0F\u200d💋\u200d👨\U0001F3FB")]
    [InlineData("👩\U0001F3FD❤\uFE0F💋👨\U0001F3FB", 0, 4, "👩\U0001F3FD", 1, 2, 3, 2, "❤\uFE0F")]
    public void TestExtractSubstring(string testValue, int index, int rIndex, string expectedGrapheme, int start, int end, int rStart, int rEnd, string expectedGraphemes)
    {
        var str = new GraphemeString(testValue);
        Assert.Equal(testValue, str.ToString());
        Assert.Equal(testValue, str.ValueSpan.ToString());
        Assert.Equal(expectedGrapheme, str[index].ToString());
        Assert.Equal(expectedGrapheme, str[^rIndex].ToString());
        Assert.Equal(expectedGraphemes, str[start..end].ToString());
        Assert.Equal(expectedGraphemes, str[^rStart..^rEnd].ToString());
        Assert.Equal(expectedGraphemes, str[start..end].ValueSpan.ToString());
        Assert.Equal(expectedGraphemes, str[^rStart..^rEnd].ValueSpan.ToString());
    }

    [Fact]
    public void TestExtractSubstringException()
    {
        var str = new GraphemeString("The quick brown fox");
        Assert.Throws<ArgumentOutOfRangeException>(() => str[-1].ToString());
        Assert.Throws<ArgumentOutOfRangeException>(() => str[str.Length + 1].ToString());
        Assert.Throws<ArgumentOutOfRangeException>(() => str[-1..2]);
        Assert.Throws<ArgumentOutOfRangeException>(() => str[0..(str.Length + 1)]);
        Assert.Throws<ArgumentOutOfRangeException>(() => str[(str.Length + 1)..]);
    }

    [Fact]
    public void TestIsEmpty()
    {
        Assert.True(GraphemeString.Empty.IsEmpty);
        Assert.True(new GraphemeString("").IsEmpty);
        Assert.False(new GraphemeString("\x20").IsEmpty);
    }

    [Fact]
    public void TestWhiteSpaces()
    {
        Assert.True(GraphemeString.Empty.IsWhiteSpace());
        Assert.True(new GraphemeString("").IsWhiteSpace());
        Assert.True(new GraphemeString("\x20").IsWhiteSpace());
        Assert.True(new GraphemeString("\x20\x20\t").IsWhiteSpace());
        Assert.True(new GraphemeString("\r\n").IsWhiteSpace());
        Assert.True(new GraphemeString("\u3000\x20\u3000").IsWhiteSpace());
        Assert.False(new GraphemeString("\x20\x20a").IsWhiteSpace());
    }

    [Theory]
    [InlineData("あいうえおがぎぐげごぱぴぷぺぽ",
        "あ", "い", "う", "え", "お", "が", "ぎ", "ぐ", "げ", "ご", "ぱ", "ぴ", "ぷ", "ぺ", "ぽ")]
    [InlineData("あいうえおか\u3099き\u3099く\u3099け\u3099こ\u3099は\u309aひ\u309aふ\u309aへ\u309aほ\u309a",
        "あ", "い", "う", "え", "お", "か\u3099", "き\u3099", "く\u3099", "け\u3099", "こ\u3099", "は\u309a", "ひ\u309a", "ふ\u309a", "へ\u309a", "ほ\u309a")]
    [InlineData("👨\u200d👩\u200d👧\u200d👦👨👩👧👦", "👨\u200d👩\u200d👧\u200d👦", "👨", "👩", "👧", "👦")]
    [InlineData("🇺🇸🇰🇷🇺🇸🇰🇷", "🇺🇸", "🇰🇷", "🇺🇸", "🇰🇷")]
    [InlineData("England🏴\U000E0067\U000E0062\U000E0065\U000E006E\U000E0067\U000E007F",
        "E", "n", "g", "l", "a", "n", "d", "🏴\U000E0067\U000E0062\U000E0065\U000E006E\U000E0067\U000E007F")]
    [InlineData("👩\U0001F3FD\u200d❤\uFE0F\u200d💋\u200d👨\U0001F3FB\r\n", "👩\U0001F3FD\u200d❤\uFE0F\u200d💋\u200d👨\U0001F3FB", "\r\n")]
    [InlineData("👩\U0001F3FD❤\uFE0F💋👨\U0001F3FB", "👩\U0001F3FD", "❤\uFE0F", "💋", "👨\U0001F3FB")]
    public void TestEnumeration(string testValue, params string[] expected)
    {
        var str = new GraphemeString(testValue);
        {
            var i = 0;
            foreach (var g in str)
            {
                Assert.Equal(expected[i++], g.ToString());
            }
        }
        {
            var i = 0;
            var enumerator = str.GetEnumerator();
            Assert.Throws<InvalidOperationException>(() => enumerator.Current.ToString());
            while (enumerator.MoveNext())
            {
                Assert.Equal(expected[i++], enumerator.Current.ToString());
            }
            Assert.Throws<InvalidOperationException>(() => enumerator.Current.ToString());

            i = 0;
            enumerator.Reset();
            Assert.Throws<InvalidOperationException>(() => enumerator.Current.ToString());
            while (enumerator.MoveNext())
            {
                Assert.Equal(expected[i++], enumerator.Current.ToString());
            }
            Assert.Throws<InvalidOperationException>(() => enumerator.Current.ToString());
        }
    }

    public static IEnumerable<object[]> LoadData(string file, string pattern)
    {
        var regex = new Regex(pattern);
        using var reader = File.OpenText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data", file));
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            {
                var match = regex.Match(line).Groups["Grapheme"];
                var graphemes = match.Value.Split("..", StringSplitOptions.RemoveEmptyEntries).Select((x, i) => (index: match.Index + i * 3, value: x)).ToArray();
                yield return new object[]
                {
                    line.Length,
                    line.Length - graphemes.Sum(x => x.value.Length - 1),
                    graphemes,
                    line
                };
            }
        }
    }
}