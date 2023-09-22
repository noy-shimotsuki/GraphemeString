using System.Text;
using TsukuyoOka.Text.Unicode.Utils;

namespace TsukuyoOka.Text.Unicode;

public class GraphemeStringConversionsTest
{
    static GraphemeStringConversionsTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static readonly string[] _testValuesA =
    {
        "0123456789",
        "The quick brown fox jumps over the lazy dog.",
        "Victor jagt zwölf Boxkämpfer quer über den großen Sylter Deich.",
        "Le cœur déçu mais l'âme plutôt naïve, Louÿs rêva de crapaüter en canoë au delà des îles, près du mälströn où brûlent les novæ.",
        "El veloz murciélago hindú comía feliz cardillo y kiwi. La cigüeña tocaba el saxofón detrás del palenque de paja.",
        "В чащах юга жил бы цитрус? Да, но фальшивый экземпляр!",
        "色は匂へど散りぬるを我が世誰ぞ常ならむ有為の奥山今日越えて浅き夢見じ酔ひもせず",
        "Quickなbrownのfoxはlazyなdogをjumpしoverする",
        "🏃\u200D♀Quick💨なbrownのfox🦊はlazy🚶\u200D♂️なdog🐩をjump🕴🏻しoverする",
        "國破山河在 城春草木深 感時花濺淚 恨別鳥驚心 烽火連三月 家書抵萬金 白頭掻更短 渾欲不勝簪",
        "国破山河在 城春草木深 感时花溅泪 恨别鸟惊心 烽火连三月 家书抵万金 白头掻更短 浑欲不胜簪"
    };

    public static IEnumerable<object[]> GetTestDataForTruncateByByteLength()
    {
        var testEncodings = new[]
        {
            Encoding.UTF8,
            Encoding.Unicode,
            Encoding.BigEndianUnicode,
            Encoding.UTF32,
            Encoding.ASCII,
            Encoding.GetEncoding("Windows-1252"),
            Encoding.GetEncoding("Shift_JIS"),
            Encoding.GetEncoding("GB18030")
        };

        return _testValuesA.SelectMany(value => testEncodings.Select(encoding => new object[] { value, encoding }));
    }

    public static IEnumerable<object[]> GetTestValuesA()
    {
        return _testValuesA.Select(value => new object[] { value });
    }

    [Theory]
    [InlineData("The quick brown fox jumps over the lazy dog.", "fox", "ferret", "The quick brown ferret jumps over the lazy dog.")]
    [InlineData("いっぱい", "い", "お", "おっぱお")]
    [InlineData("🐭🐮🐯🐰🐲🐍🐴🐏🐵🐔🐶🐗", "🐗", "🐷", "🐭🐮🐯🐰🐲🐍🐴🐏🐵🐔🐶🐷")]
    [InlineData("👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧", "👩", "👨", "👨\u200D👩\u200D👧\u200D👧👨👨👧👧👨\u200D👩\u200D👧\u200D👧")]
    [InlineData("The quick brown fox jumps over the lazy dog.", "fox", "fox", "The quick brown fox jumps over the lazy dog.")]
    [InlineData("👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧", "👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧", "👨",
        "👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧")]
    [InlineData("fox", "The quick brown fox jumps over the lazy dog.", "ferret", "fox")]
    [InlineData("The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.", "T", "t",
        "the quick brown fox jumps over the lazy dog. the quick brown fox jumps over the lazy dog.")]
    [InlineData("The quick brown fox", "The quick brown fox", "", "")]
    public void TestReplace(string testValue, string oldValue, string newValue, string expectedValue)
    {
        var str = new GraphemeString(testValue);
        Assert.Throws<ArgumentException>(() => str.Replace("", newValue, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(expectedValue, str.Replace(oldValue, newValue, StringComparison.Ordinal).ToString());
        var str2 = new GraphemeString("🧑" + testValue + "🧑")[1..^1];
        Assert.Throws<ArgumentException>(() => str2.Replace("", newValue, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(expectedValue, str2.Replace(oldValue, newValue, StringComparison.Ordinal).ToString());
    }

    [Theory]
    [InlineData("The quick brown fox jumps over the lazy dog.", " ", StringComparison.Ordinal, -1, StringSplitOptions.None,
        "The", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog.")]
    [InlineData("The quick brown fox jumps over the lazy dog.", " ", StringComparison.Ordinal, 3, StringSplitOptions.None,
        "The", "quick", "brown fox jumps over the lazy dog.")]
    [InlineData("👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧", "👧", StringComparison.Ordinal, -1, StringSplitOptions.None,
        "👨\u200D👩\u200D👧\u200D👧👨👩", "", "👨\u200D👩\u200D👧\u200D👧")]
    [InlineData("👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧", "👧", StringComparison.Ordinal, -1, StringSplitOptions.RemoveEmptyEntries,
        "👨\u200D👩\u200D👧\u200D👧👨👩", "👨\u200D👩\u200D👧\u200D👧")]
    [InlineData("👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧", "👨\u200D👩\u200D👧\u200D👧👨👩", StringComparison.Ordinal, -1, StringSplitOptions.None,
        "", "👧👧👨\u200D👩\u200D👧\u200D👧")]
    [InlineData("The quick brown fox jumps over the lazy dog.", "fox", StringComparison.Ordinal, -1, StringSplitOptions.None,
        "The quick brown ", " jumps over the lazy dog.")]
    [InlineData("The quick brown fox jumps over the lazy dog.", "FOX", StringComparison.Ordinal, -1, StringSplitOptions.None,
        "The quick brown fox jumps over the lazy dog.")]
    [InlineData("The quick brown fox jumps over the lazy dog.", "FOX", StringComparison.OrdinalIgnoreCase, -1, StringSplitOptions.None,
        "The quick brown ", " jumps over the lazy dog.")]
    [InlineData(" The quick brown fox jumps over the lazy dog. ", "fox", StringComparison.Ordinal, -1, StringSplitOptions.TrimEntries,
        "The quick brown", "jumps over the lazy dog.")]
    [InlineData("The quick brown fox jumps over the lazy dog.", "The", StringComparison.Ordinal, -1, StringSplitOptions.None,
        "", " quick brown fox jumps over the lazy dog.")]
    [InlineData(" The quick brown fox jumps over the lazy dog. ", "The", StringComparison.Ordinal, -1, StringSplitOptions.TrimEntries,
        "", "quick brown fox jumps over the lazy dog.")]
    [InlineData("The quick brown fox jumps over the lazy dog. ", "The", StringComparison.Ordinal, -1, StringSplitOptions.RemoveEmptyEntries,
        " quick brown fox jumps over the lazy dog. ")]
    [InlineData(" The quick brown fox jumps over the lazy dog. ", "The", StringComparison.Ordinal, -1, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries,
        "quick brown fox jumps over the lazy dog.")]
    [InlineData("The quick brown fox jumps over the lazy dog.", "", StringComparison.Ordinal, -1, StringSplitOptions.None,
        "The quick brown fox jumps over the lazy dog.")]
    [InlineData("The quick brown fox jumps over the lazy dog.", " ", StringComparison.Ordinal, 1, StringSplitOptions.None,
        "The quick brown fox jumps over the lazy dog.")]
    [InlineData("ABC abc ABC", "abc", StringComparison.Ordinal, -1, StringSplitOptions.None, "ABC ", " ABC")]
    [InlineData("ABC abc ABC", "abc", StringComparison.OrdinalIgnoreCase, -1, StringSplitOptions.None, "", " ", " ", "")]
    [InlineData("ABC abc ABC", "abc", StringComparison.OrdinalIgnoreCase, -1, StringSplitOptions.TrimEntries, "", "", "", "")]
    [InlineData("ABC abc ABC", "abc", StringComparison.OrdinalIgnoreCase, -1, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)]
    [InlineData("  ", "", StringComparison.Ordinal, -1, StringSplitOptions.TrimEntries, "")]
    [InlineData("  ", "a", StringComparison.Ordinal, 1, StringSplitOptions.TrimEntries, "")]
    [InlineData("  ", "", StringComparison.Ordinal, -1, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)]
    [InlineData("The quick brown fox", " ", StringComparison.Ordinal, 0, StringSplitOptions.None)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "CDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwx", StringComparison.Ordinal, -1, StringSplitOptions.None,
        "AB", "yz")]
    public void TestSplit(string testValue, string testSeparator, StringComparison stringComparison, int count, StringSplitOptions stringSplitOptions, params string[] expected)
    {
        var str = new GraphemeString(testValue);
        var str2 = new GraphemeString("🧑" + testValue + "🧑")[1..^1];
        if (count < 0)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => str.Split(testSeparator, stringComparison, count, stringSplitOptions));
            Assert.Equal(expected, str.Split(testSeparator, stringComparison, stringSplitOptions).Select(x => x.ToString()));
            Assert.Equal(expected, str.Split(testSeparator, stringComparison, stringSplitOptions).Select(x => x.ValueSpan.ToString()));
            Assert.Throws<ArgumentOutOfRangeException>(() => str2.Split(testSeparator, stringComparison, count, stringSplitOptions));
            Assert.Equal(expected, str2.Split(testSeparator, stringComparison, stringSplitOptions).Select(x => x.ToString()));
            Assert.Equal(expected, str2.Split(testSeparator, stringComparison, stringSplitOptions).Select(x => x.ValueSpan.ToString()));
        }
        else
        {
            Assert.Equal(expected, str.Split(testSeparator, stringComparison, count, stringSplitOptions).Select(x => x.ToString()));
            Assert.Equal(expected, str.Split(testSeparator, stringComparison, count, stringSplitOptions).Select(x => x.ValueSpan.ToString()));
            Assert.Equal(expected, str2.Split(testSeparator, stringComparison, count, stringSplitOptions).Select(x => x.ToString()));
            Assert.Equal(expected, str2.Split(testSeparator, stringComparison, count, stringSplitOptions).Select(x => x.ValueSpan.ToString()));
        }
    }

    [Theory]
    [InlineData("The quick brown fox", "Thx", "e quick brown fo")]
    [InlineData("👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨", "👨👩👧",
        "👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧")]
    [InlineData("👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨", "👨👩👧\u200D",
        "👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧")]
    [InlineData("👨👩👧👧", "\uD83D", "👨👩👧👧")]
    [InlineData("👨👩👧👧", "👨👩👧👧", "")]
    [InlineData("👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨", "👨👩👧👧👨\u200D👩\u200D👧\u200D👧", "")]
    [InlineData(" The quick brown fox ", "", " The quick brown fox ")]
    [InlineData("いろはにほへとちりぬるをわかよたれそつねならむうゐのおくやまけふこえてあさきゆめみしゑひもせす",
        "いろはにほへとちりぬるをわかよたれそつねならむうのおくやまけふこえてあさきゆめみしゑひもせす", "ゐ")]
    [InlineData("", "The", "")]
    public void TestTrim(string testValue, string trimValue, string expected)
    {
        Assert.Equal(expected, new GraphemeString(testValue).Trim(trimValue, StringComparison.Ordinal).ToString());
        Assert.Equal(expected, new GraphemeString("🧑" + testValue + "🧑")[1..^1].Trim(trimValue, StringComparison.Ordinal).ToString());
    }

    [Theory]
    [InlineData("The quick brown fox", "Thx", "The quick brown fo")]
    [InlineData("👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨", "👨👩👧",
        "👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧")]
    [InlineData("👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨", "👨👩👧\u200D",
        "👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧")]
    [InlineData("👨👩👧👧", "\uD83D", "👨👩👧👧")]
    [InlineData("👨👩👧👧", "👨👩👧👧", "")]
    [InlineData("👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨", "👨👩👧👧👨\u200D👩\u200D👧\u200D👧", "")]
    [InlineData(" The quick brown fox ", "", " The quick brown fox ")]
    [InlineData("いろはにほへとちりぬるをわかよたれそつねならむうゐのおくやまけふこえてあさきゆめみしゑひもせす",
        "いろはにほへとちりぬるをわかよたれそつねならむうのおくやまけふこえてあさきゆめみしゑひもせす", "いろはにほへとちりぬるをわかよたれそつねならむうゐ")]
    [InlineData("", "The", "")]
    public void TestTrimEnd(string testValue, string trimValue, string expected)
    {
        Assert.Equal(expected, new GraphemeString(testValue).TrimEnd(trimValue, StringComparison.Ordinal).ToString());
        Assert.Equal(expected, new GraphemeString("🧑" + testValue + "🧑")[1..^1].TrimEnd(trimValue, StringComparison.Ordinal).ToString());
    }

    [Theory]
    [InlineData("  The  quick    brown fox ", "  The  quick    brown fox")]
    [InlineData("\u3000 \t\r\nThe quick  brown fox \xA0\u200A ", "\u3000 \t\r\nThe quick  brown fox")]
    [InlineData(" The quick brown fox ", " The quick brown fox")]
    [InlineData(" The quick brown fox", " The quick brown fox")]
    [InlineData("", "")]
    [InlineData("    ", "")]
    public void TestTrimEndWhitespaces(string testValue, string expected)
    {
        Assert.Equal(expected, new GraphemeString(testValue).TrimEnd().ToString());
        Assert.Equal(expected, new GraphemeString("🧑" + testValue + "🧑")[1..^1].TrimEnd().ToString());
    }

    [Theory]
    [InlineData("The quick brown fox", "Thx", "e quick brown fox")]
    [InlineData("👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨", "👨👩👧",
        "👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨")]
    [InlineData("👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨", "👨👩👧\u200D",
        "👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨")]
    [InlineData("👨👩👧👧", "\uD83D", "👨👩👧👧")]
    [InlineData("👨👩👧👧", "👨👩👧👧", "")]
    [InlineData("👨👩👧👧👨\u200D👩\u200D👧\u200D👧👨👩👧👧👨\u200D👩\u200D👧\u200D👧👧👧👩👨", "👨👩👧👧👨\u200D👩\u200D👧\u200D👧", "")]
    [InlineData(" The quick brown fox ", "", " The quick brown fox ")]
    [InlineData("いろはにほへとちりぬるをわかよたれそつねならむうゐのおくやまけふこえてあさきゆめみしゑひもせす",
        "いろはにほへとちりぬるをわかよたれそつねならむうのおくやまけふこえてあさきゆめみしゑひもせす", "ゐのおくやまけふこえてあさきゆめみしゑひもせす")]
    [InlineData("", "The", "")]
    public void TestTrimStart(string testValue, string trimValue, string expected)
    {
        Assert.Equal(expected, new GraphemeString(testValue).TrimStart(trimValue, StringComparison.Ordinal).ToString());
        Assert.Equal(expected, new GraphemeString("🧑" + testValue + "🧑")[1..^1].TrimStart(trimValue, StringComparison.Ordinal).ToString());
    }

    [Theory]
    [InlineData("  The  quick    brown fox ", "The  quick    brown fox ")]
    [InlineData("\u3000 \t\r\nThe quick  brown fox \xA0\u200A ", "The quick  brown fox \xA0\u200A ")]
    [InlineData(" The quick brown fox ", "The quick brown fox ")]
    [InlineData("The quick brown fox ", "The quick brown fox ")]
    [InlineData("", "")]
    [InlineData("    ", "")]
    public void TestTrimStartWhitespaces(string testValue, string expected)
    {
        Assert.Equal(expected, new GraphemeString(testValue).TrimStart().ToString());
        Assert.Equal(expected, new GraphemeString("🧑" + testValue + "🧑")[1..^1].TrimStart().ToString());
    }

    [Theory]
    [InlineData("  The  quick    brown fox ", "The  quick    brown fox")]
    [InlineData("\u3000 \t\r\nThe quick  brown fox \xA0\u200A ", "The quick  brown fox")]
    [InlineData("The quick brown fox ", "The quick brown fox")]
    [InlineData(" The quick brown fox", "The quick brown fox")]
    [InlineData("The quick brown fox", "The quick brown fox")]
    [InlineData("", "")]
    [InlineData("    ", "")]
    public void TestTrimWhitespaces(string testValue, string expected)
    {
        Assert.Equal(expected, new GraphemeString(testValue).Trim().ToString());
        Assert.Equal(expected, new GraphemeString("🧑" + testValue + "🧑")[1..^1].Trim().ToString());
    }

    [Theory]
    [MemberData(nameof(GetTestDataForTruncateByByteLength))]
    public void TestTruncateByByteLength(string testValue, Encoding encoding)
    {
        var str = new GraphemeString(testValue);
        var str2 = new GraphemeString("🧑" + testValue + "🧑")[1..^1];
        Assert.Throws<ArgumentOutOfRangeException>(() => str.TruncateByByteLength(encoding, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => testValue.TruncateByByteLength(encoding, -1));
        for (var i = 0; i < str.CharLength * 5; i++)
        {
            var result = str.TruncateByByteLength(encoding, i);
            var result2 = testValue.TruncateByByteLength(encoding, i);
            AssertUtil.LowerThanOrEquals(i, encoding.GetBytes(result.ToString()).Length);
            AssertUtil.LowerThanOrEquals(i, encoding.GetBytes(result2).Length);
            if (str != result)
            {
                AssertUtil.GreaterThan(i, encoding.GetBytes(str[..(result.Length + 1)].ToString()).Length);
            }

            if (testValue != result2)
            {
                AssertUtil.GreaterThan(i, encoding.GetBytes(str[..(result2.CountGraphemes() + 1)].ToString()).Length);
            }

            Assert.True(result == str2.TruncateByByteLength(encoding, i));
        }
    }

    [Theory]
    [MemberData(nameof(GetTestValuesA))]
    public void TestTruncateByCharLength(string testValue)
    {
        var str = new GraphemeString(testValue);
        var str2 = new GraphemeString("🧑" + testValue + "🧑")[1..^1];
        Assert.Throws<ArgumentOutOfRangeException>(() => str.TruncateByCharLength(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => testValue.TruncateByCharLength(-1));
        for (var i = 0; i < str.CharLength * 2; i++)
        {
            var result = str.TruncateByCharLength(i);
            var result2 = testValue.TruncateByCharLength(i);
            AssertUtil.LowerThanOrEquals(i, result.ValueSpan.Length);
            AssertUtil.LowerThanOrEquals(i, result2.Length);
            if (str != result)
            {
                AssertUtil.GreaterThan(i, str[..(result.Length + 1)].ValueSpan.Length);
            }

            if (testValue != result2)
            {
                AssertUtil.GreaterThan(i, str[..(result2.CountGraphemes() + 1)].ValueSpan.Length);
            }

            Assert.True(result == str2.TruncateByCharLength(i));
        }
    }

    [Theory]
    [MemberData(nameof(GetTestValuesA))]
    public void TestTruncateByCodePointLength(string testValue)
    {
        var str = new GraphemeString(testValue);
        var str2 = new GraphemeString("🧑" + testValue + "🧑")[1..^1];
        Assert.Throws<ArgumentOutOfRangeException>(() => str.TruncateByCodePointLength(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => testValue.TruncateByCodePointLength(-1));
        for (var i = 0; i < str.CharLength * 3; i++)
        {
            var result = str.TruncateByCodePointLength(i);
            var result2 = testValue.TruncateByCodePointLength(i);
            AssertUtil.LowerThanOrEquals(i, result.ToString().EnumerateRunes().Count());
            AssertUtil.LowerThanOrEquals(i, result2.EnumerateRunes().Count());
            if (str != result)
            {
                AssertUtil.GreaterThan(i, str[..(result.Length + 1)].ToString().EnumerateRunes().Count());
            }

            if (testValue != result2)
            {
                AssertUtil.GreaterThan(i, str[..(result2.CountGraphemes() + 1)].ToString().EnumerateRunes().Count());
            }

            Assert.True(result == str2.TruncateByCodePointLength(i));
        }
    }

    [Theory]
    [MemberData(nameof(GetTestValuesA))]
    public void TestTruncateByGraphemeLength(string testValue)
    {
        var str = new GraphemeString(testValue);
        var str2 = new GraphemeString("🧑" + testValue + "🧑")[1..^1];
        Assert.Throws<ArgumentOutOfRangeException>(() => str.TruncateByGraphemeLength(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => testValue.TruncateByGraphemeLength(-1));
        for (var i = 0; i < str.CharLength * 3; i++)
        {
            var result = str.TruncateByGraphemeLength(i);
            var result2 = testValue.TruncateByGraphemeLength(i);
            AssertUtil.LowerThanOrEquals(i, result.Length);
            AssertUtil.LowerThanOrEquals(i, result2.CountGraphemes());
            if (str != result)
            {
                AssertUtil.GreaterThan(i, str[..(result.Length + 1)].Length);
            }

            if (testValue != result2)
            {
                AssertUtil.GreaterThan(i, str[..(result2.CountGraphemes() + 1)].Length);
            }

            Assert.True(result == str2.TruncateByGraphemeLength(i));
        }
    }
}