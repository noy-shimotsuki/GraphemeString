namespace TsukuyoOka.Text.Unicode;

public sealed partial class GraphemeString
{
    /// <summary>
    /// Determines if two values are the same using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(GraphemeString? left, GraphemeString? right)
    {
        return (left is null && right is null) || (left is not null && left.Equals(right, StringComparison.Ordinal));
    }

    /// <summary>
    /// Determines if two values are the same using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(GraphemeString? left, ReadOnlySpan<char> right)
    {
        return left is not null && left.Equals(right, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines if two values are the same using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(ReadOnlySpan<char> left, GraphemeString? right)
    {
        return right == left;
    }

    /// <summary>
    /// Determines if two values are not the same using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(GraphemeString? left, GraphemeString? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Determines if two values are not the same using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(GraphemeString? left, ReadOnlySpan<char> right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Determines if two values are not the same using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(ReadOnlySpan<char> left, GraphemeString? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator <(GraphemeString? left, GraphemeString? right)
    {
        return (left is null && right is not null) || (left is not null && left.CompareTo(right, StringComparison.Ordinal) < 0);
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator <(GraphemeString? left, ReadOnlySpan<char> right)
    {
        return left is null || left.CompareTo(right, StringComparison.Ordinal) < 0;
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator <(ReadOnlySpan<char> left, GraphemeString? right)
    {
        return right is not null && right.CompareTo(left, StringComparison.Ordinal) > 0;
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator <=(GraphemeString? left, GraphemeString? right)
    {
        return left is null || left.CompareTo(right, StringComparison.Ordinal) <= 0;
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator <=(GraphemeString? left, ReadOnlySpan<char> right)
    {
        return left is null || left.CompareTo(right, StringComparison.Ordinal) <= 0;
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator <=(ReadOnlySpan<char> left, GraphemeString? right)
    {
        return right is not null && right.CompareTo(left, StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator >(GraphemeString? left, GraphemeString? right)
    {
        return left is not null && left.CompareTo(right, StringComparison.Ordinal) > 0;
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator >(GraphemeString? left, ReadOnlySpan<char> right)
    {
        return left is not null && left.CompareTo(right, StringComparison.Ordinal) > 0;
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator >(ReadOnlySpan<char> left, GraphemeString? right)
    {
        return right is null || right.CompareTo(left, StringComparison.Ordinal) < 0;
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator >=(GraphemeString? left, GraphemeString? right)
    {
        return right is null || (left is not null && left.CompareTo(right, StringComparison.Ordinal) >= 0);
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator >=(GraphemeString? left, ReadOnlySpan<char> right)
    {
        return left is not null && left.CompareTo(right, StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Compares two values using ordinal (binary) sort rules.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator >=(ReadOnlySpan<char> left, GraphemeString? right)
    {
        return right is null || right.CompareTo(left, StringComparison.Ordinal) <= 0;
    }
}