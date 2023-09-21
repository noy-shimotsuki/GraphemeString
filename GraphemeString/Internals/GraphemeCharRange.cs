namespace TsukuyoOka.Text.Unicode.Internals;

internal readonly struct GraphemeCharRange : IEquatable<GraphemeCharRange>
{
    public int Start { get; }
    public int Length { get; }
    public int End => Start + Length;

    public GraphemeCharRange(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public bool Equals(GraphemeCharRange other)
    {
        return Start == other.Start && Length == other.Length;
    }

    public override bool Equals(object? obj)
    {
        return obj is GraphemeCharRange charRange && Equals(charRange);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, Length);
    }
}