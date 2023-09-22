namespace TsukuyoOka.Text.Unicode;

public class GraphemeStringOperatorsTest
{
    [Fact]
    public void TestOpEquality()
    {
        Assert.True(new GraphemeString("hoge") == new GraphemeString("hoge"));
        Assert.True(new GraphemeString("hoge") == "hoge");
        Assert.True("hoge" == new GraphemeString("hoge"));

        Assert.True(new GraphemeString("fuga") == new GraphemeString("hogefugapiyo", 4..8));
        Assert.True(new GraphemeString("hogefugapiyo", 4..8) == "fuga");
        Assert.False(new GraphemeString("hoge") == new GraphemeString("Hoge"));

        Assert.True((GraphemeString?)null == (GraphemeString?)null);
        Assert.False((GraphemeString?)null == new GraphemeString("hoge"));
        Assert.False(new GraphemeString("hoge") == (GraphemeString?)null);
        Assert.False((GraphemeString?)null == "");
        Assert.False("" == (GraphemeString?)null);

        Assert.True(GraphemeString.Empty == new GraphemeString(""));
    }

    [Fact]
    public void TestOpNotInequality()
    {
        Assert.False(new GraphemeString("hoge") != new GraphemeString("hoge"));
        Assert.False(new GraphemeString("hoge") != "hoge");
        Assert.False("hoge" != new GraphemeString("hoge"));

        Assert.False(new GraphemeString("fuga") != new GraphemeString("hogefugapiyo", 4..8));
        Assert.False(new GraphemeString("hogefugapiyo", 4..8) != "fuga");
        Assert.True(new GraphemeString("hoge") != new GraphemeString("Hoge"));

        Assert.False((GraphemeString?)null != (GraphemeString?)null);
        Assert.True((GraphemeString?)null != new GraphemeString("hoge"));
        Assert.True(new GraphemeString("hoge") != (GraphemeString?)null);
        Assert.True((GraphemeString?)null != "");
        Assert.True("" != (GraphemeString?)null);

        Assert.False(GraphemeString.Empty != new GraphemeString(""));
    }

    [Fact]
    public void TestOpLessThan()
    {
        Assert.True(new GraphemeString("hoge") < new GraphemeString("hoge1"));
        Assert.False(new GraphemeString("hoge") < new GraphemeString("hoge"));
        Assert.False(new GraphemeString("hoge1") < new GraphemeString("hoge"));
        Assert.True(new GraphemeString("hoge") < "hoge1");
        Assert.False(new GraphemeString("hoge") < "hoge");
        Assert.False(new GraphemeString("hoge1") < "hoge");
        Assert.True("hoge" < new GraphemeString("hoge1"));
        Assert.False("hoge" < new GraphemeString("hoge"));
        Assert.False("hoge1" < new GraphemeString("hoge"));
        Assert.True((GraphemeString?)null < new GraphemeString("hoge"));
        Assert.False(new GraphemeString("hoge") < (GraphemeString?)null);
        Assert.True((GraphemeString?)null < "hoge");
        Assert.False("hoge" < (GraphemeString?)null);
        Assert.False((GraphemeString?)null < (GraphemeString?)null);
    }

    [Fact]
    public void TestOpLessThanOrEqual()
    {
        Assert.True(new GraphemeString("hoge") <= new GraphemeString("hoge1"));
        Assert.True(new GraphemeString("hoge") <= new GraphemeString("hoge"));
        Assert.False(new GraphemeString("hoge1") <= new GraphemeString("hoge"));
        Assert.True(new GraphemeString("hoge") <= "hoge1");
        Assert.True(new GraphemeString("hoge") <= "hoge");
        Assert.False(new GraphemeString("hoge1") <= "hoge");
        Assert.True("hoge" <= new GraphemeString("hoge1"));
        Assert.True("hoge" <= new GraphemeString("hoge"));
        Assert.False("hoge1" <= new GraphemeString("hoge"));
        Assert.True((GraphemeString?)null <= new GraphemeString("hoge"));
        Assert.False(new GraphemeString("hoge") <= (GraphemeString?)null);
        Assert.True((GraphemeString?)null <= "hoge");
        Assert.False("hoge" <= (GraphemeString?)null);
        Assert.True((GraphemeString?)null <= (GraphemeString?)null);
    }

    [Fact]
    public void TestOpGreaterThan()
    {
        Assert.False(new GraphemeString("hoge") > new GraphemeString("hoge1"));
        Assert.False(new GraphemeString("hoge") > new GraphemeString("hoge"));
        Assert.True(new GraphemeString("hoge1") > new GraphemeString("hoge"));
        Assert.False(new GraphemeString("hoge") > "hoge1");
        Assert.False(new GraphemeString("hoge") > "hoge");
        Assert.True(new GraphemeString("hoge1") > "hoge");
        Assert.False("hoge" > new GraphemeString("hoge1"));
        Assert.False("hoge" > new GraphemeString("hoge"));
        Assert.True("hoge1" > new GraphemeString("hoge"));
        Assert.False((GraphemeString?)null > new GraphemeString("hoge"));
        Assert.True(new GraphemeString("hoge") > (GraphemeString?)null);
        Assert.False((GraphemeString?)null > "hoge");
        Assert.True("hoge" > (GraphemeString?)null);
        Assert.False((GraphemeString?)null > (GraphemeString?)null);
    }

    [Fact]
    public void TestOpGreaterThanOrEqual()
    {
        Assert.False(new GraphemeString("hoge") >= new GraphemeString("hoge1"));
        Assert.True(new GraphemeString("hoge") >= new GraphemeString("hoge"));
        Assert.True(new GraphemeString("hoge1") >= new GraphemeString("hoge"));
        Assert.False(new GraphemeString("hoge") >= "hoge1");
        Assert.True(new GraphemeString("hoge") >= "hoge");
        Assert.True(new GraphemeString("hoge1") >= "hoge");
        Assert.False("hoge" >= new GraphemeString("hoge1"));
        Assert.True("hoge" >= new GraphemeString("hoge"));
        Assert.True("hoge1" >= new GraphemeString("hoge"));
        Assert.False((GraphemeString?)null >= new GraphemeString("hoge"));
        Assert.True(new GraphemeString("hoge") >= (GraphemeString?)null);
        Assert.False((GraphemeString?)null >= "hoge");
        Assert.True("hoge" >= (GraphemeString?)null);
        Assert.True((GraphemeString?)null >= (GraphemeString?)null);
    }
}