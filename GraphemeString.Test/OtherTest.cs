using TsukuyoOka.Text.Unicode.Internals;

namespace TsukuyoOka.Text.Unicode;

public class OtherTest
{
    [Fact]
    public void TestGraphemeCharRange()
    {
        Assert.True(new GraphemeCharRange(0, 5).GetHashCode() == new GraphemeCharRange(0, 5).GetHashCode());
        Assert.False(new GraphemeCharRange(0, 5).GetHashCode() == new GraphemeCharRange(0, 4).GetHashCode());
        Assert.False(new GraphemeCharRange(0, 5).GetHashCode() == new GraphemeCharRange(1, 5).GetHashCode());
        Assert.True(new GraphemeCharRange(0, 5).Equals(new GraphemeCharRange(0, 5)));
        Assert.False(new GraphemeCharRange(0, 5).Equals(new GraphemeCharRange(0, 4)));
        Assert.False(new GraphemeCharRange(0, 5).Equals(new GraphemeCharRange(1, 5)));
        Assert.True(new GraphemeCharRange(0, 5).Equals((object?)new GraphemeCharRange(0, 5)));
        Assert.False(new GraphemeCharRange(0, 5).Equals((object?)new GraphemeCharRange(0, 4)));
        Assert.False(new GraphemeCharRange(0, 5).Equals(new object()));
    }

    [Fact]
    public void TestGraphemeStringCoreGetValueSpan()
    {
        Assert.Equal("", new GraphemeStringCore("test📝value", 0, 11).GetValueSpan(0, 0).ToString());
        Assert.Throws<InvalidOperationException>(() => new GraphemeStringCore("test📝value", 0, 11).GetValueSpan(1, null).ToString());
        Assert.Throws<InvalidOperationException>(() => new GraphemeStringCore("test📝value", 0, 11).GetValue(1, null).ToString());
    }
}