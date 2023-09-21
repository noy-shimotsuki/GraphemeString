using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using TsukuyoOka.Text.Unicode.Internals;

namespace TsukuyoOka.Text.Unicode;

public partial class GraphemeString : IEquatable<GraphemeString>, IComparable<GraphemeString>
{
    /// <summary>
    /// Reports the zero-based character index of the first occurrence of the specified <paramref name="value"/> in this
    /// string.
    /// </summary>
    /// <param name="value">The search string.</param>
    /// <param name="stringComparison">One of the enumeration values to specify the rules for this search.</param>
    /// <returns>
    /// The zero-based index position of <paramref name="value"/> if that string is found, or -1 if it is not. If
    /// <paramref name="value"/> is empty, the return value is 0.
    /// </returns>
    public int CharIndexOf(GraphemeString value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return 0;
        }

        if (IsEmpty)
        {
            return -1;
        }

        return _core.GetGraphemeSpan(_start, Length).CharIndexOf(value._core.GetGraphemeSpan(value._start, value.Length), stringComparison);
    }

    /// <summary>
    /// Reports the zero-based character index of the first occurrence of the specified <paramref name="value"/> in this
    /// string.
    /// </summary>
    /// <param name="value">The search string span.</param>
    /// <param name="stringComparison">One of the enumeration values to specify the rules for this search.</param>
    /// <returns>
    /// The zero-based index position of <paramref name="value"/> if that string is found, or -1 if it is not. If
    /// <paramref name="value"/> is empty, the return value is 0.
    /// </returns>
    public int CharIndexOf(ReadOnlySpan<char> value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return 0;
        }

        if (IsEmpty)
        {
            return -1;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = value.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[value.Length] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(value.Length);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            return _core.GetGraphemeSpan(_start, Length).CharIndexOf(new GraphemeSpan(value, graphemes, indices, value.Length), stringComparison);
        }
        finally
        {
            if (pooledGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledGraphemesArray);
            }
            if (pooledIndicesArray is not null)
            {
                ArrayPool<int>.Shared.Return(pooledIndicesArray);
            }
        }
    }

    /// <summary>
    /// Compares this instance with the specified <paramref name="other"/> object using the specified
    /// <paramref name="stringComparison"/> and returns an integer indicating their relative position.
    /// </summary>
    /// <param name="other">The value to compare with this instance.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// An integer indicating the relative position. Returns less than zero if this instance is less than
    /// <paramref name="other"/>, zero if equal, or greater than zero if greater.
    /// </returns>
    public int CompareTo(GraphemeString? other, StringComparison stringComparison)
    {
        return other is not null ? CompareTo(other.ValueSpan, stringComparison) : 1;
    }

    /// <summary>
    /// Compares and returns whether this instance is equal to the specified <paramref name="other"/> object using the
    /// specified <paramref name="stringComparison"/>.
    /// Compares this instance with the specified <paramref name="other"/> object using the specified
    /// <paramref name="stringComparison"/> and returns an integer indicating their relative position.
    /// </summary>
    /// <param name="other">The value to compare with this instance.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// An integer indicating the relative position. Returns less than zero if this instance is less than
    /// <paramref name="other"/>, zero if equal, or greater than zero if greater.
    /// </returns>
    public int CompareTo(ReadOnlySpan<char> other, StringComparison stringComparison)
    {
        return ValueSpan.CompareTo(other, stringComparison);
    }

    /// <summary>
    /// Determines if this string contains the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The string to compare.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// <see langword="true"/> if this string contains the specified <paramref name="value"/>; otherwise,
    /// <see langword="false"/>
    /// </returns>
    public bool Contains(GraphemeString value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return true;
        }

        if (IsEmpty)
        {
            return false;
        }

        return _core.GetGraphemeSpan(_start, Length).IndexOf(value._core.GetGraphemeSpan(value._start, value.Length), 0, stringComparison).Index >= 0;
    }

    /// <summary>
    /// Determines if this string contains the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The string to compare.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// <see langword="true"/> if this string contains the specified <paramref name="value"/>; otherwise,
    /// <see langword="false"/>
    /// </returns>
    public bool Contains(ReadOnlySpan<char> value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return true;
        }

        if (IsEmpty)
        {
            return false;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = value.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[value.Length] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(value.Length);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            return _core.GetGraphemeSpan(_start, Length).IndexOf(new GraphemeSpan(value, graphemes, indices, value.Length), 0, stringComparison).Index >= 0;
        }
        finally
        {
            if (pooledGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledGraphemesArray);
            }
            if (pooledIndicesArray is not null)
            {
                ArrayPool<int>.Shared.Return(pooledIndicesArray);
            }
        }
    }

    /// <summary>
    /// Determines if this string ends with the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The string to compare.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// <see langword="true"/> if this string ends with the specified <paramref name="value"/>; otherwise,
    /// <see langword="false"/>
    /// </returns>
    public bool EndsWith(GraphemeString value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return true;
        }

        if (IsEmpty)
        {
            return false;
        }

        return _core.GetGraphemeSpan(_start, Length).EndsWith(value._core.GetGraphemeSpan(value._start, value.Length), stringComparison);
    }

    /// <summary>
    /// Determines if this string ends with the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The string to compare.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// <see langword="true"/> if this string ends with the specified <paramref name="value"/>; otherwise,
    /// <see langword="false"/>
    /// </returns>
    public bool EndsWith(ReadOnlySpan<char> value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return true;
        }

        if (IsEmpty)
        {
            return false;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = value.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[value.Length] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(value.Length);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            return _core.GetGraphemeSpan(_start, Length).EndsWith(new GraphemeSpan(value, graphemes, indices, value.Length), stringComparison);
        }
        finally
        {
            if (pooledGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledGraphemesArray);
            }
            if (pooledIndicesArray is not null)
            {
                ArrayPool<int>.Shared.Return(pooledIndicesArray);
            }
        }
    }

    /// <summary>
    /// Determines whether this instance and the specified <paramref name="other"/> object have the same value.
    /// </summary>
    /// <param name="other">The value to compare with this instance.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="other"/> and this object are same value; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(GraphemeString? other, StringComparison stringComparison)
    {
        return other is not null && ValueSpan.Equals(other.ValueSpan, stringComparison);
    }

    /// <summary>
    /// Determines whether this instance and the specified <paramref name="other"/> object have the same value.
    /// </summary>
    /// <param name="other">The value to compare with this instance.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="other"/> and this object are same value; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(ReadOnlySpan<char> other, StringComparison stringComparison)
    {
        return ValueSpan.Equals(other, stringComparison);
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not GraphemeString)
        {
            return false;
        }

        return Equals((GraphemeString)obj, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        var span = ValueSpan;
        var value = (uint)CharLength;
        foreach (var c in span.Length > 32 ? span[..32] : span)
        {
            value = BitOperations.RotateLeft(value + c, 11);
        }

        return (int)value;
    }

    /// <summary>
    /// Reports the zero-based grapheme index of the first occurrence of the specified <paramref name="value"/> in this string.
    /// </summary>
    /// <param name="value">The search string.</param>
    /// <param name="stringComparison">One of the enumeration values to specify the rules for this search.</param>
    /// <returns>
    /// The zero-based index position of <paramref name="value"/> if that string is found, or -1 if it is not. If
    /// <paramref name="value"/> is empty, the return value is 0.
    /// </returns>
    public int IndexOf(GraphemeString value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return 0;
        }

        if (IsEmpty)
        {
            return -1;
        }

        return _core.GetGraphemeSpan(_start, Length).IndexOf(value._core.GetGraphemeSpan(value._start, value.Length), 0, stringComparison).Index;
    }

    /// <summary>
    /// Reports the zero-based grapheme index of the first occurrence of the specified <paramref name="value"/> in this string.
    /// </summary>
    /// <param name="value">The search string span.</param>
    /// <param name="stringComparison">One of the enumeration values to specify the rules for this search.</param>
    /// <returns>
    /// The zero-based index position of <paramref name="value"/> if that string is found, or -1 if it is not. If
    /// <paramref name="value"/> is empty, the return value is 0.
    /// </returns>
    public int IndexOf(ReadOnlySpan<char> value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return 0;
        }

        if (IsEmpty)
        {
            return -1;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = value.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[value.Length] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(value.Length);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            return _core.GetGraphemeSpan(_start, Length).IndexOf(new GraphemeSpan(value, graphemes, indices, value.Length), 0, stringComparison).Index;
        }
        finally
        {
            if (pooledGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledGraphemesArray);
            }
            if (pooledIndicesArray is not null)
            {
                ArrayPool<int>.Shared.Return(pooledIndicesArray);
            }
        }
    }

    /// <summary>
    /// Reports the zero-based character index of the last occurrence of the specified <paramref name="value"/> in this string.
    /// </summary>
    /// <param name="value">The search string.</param>
    /// <param name="stringComparison">One of the enumeration values to specify the rules for this search.</param>
    /// <returns>
    /// The zero-based index position of <paramref name="value"/> if that string is found, or -1 if it is not. If
    /// <paramref name="value"/> is empty, the return value is 0.
    /// </returns>
    public int LastCharIndexOf(GraphemeString value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return CharLength;
        }

        if (IsEmpty)
        {
            return -1;
        }

        return _core.GetGraphemeSpan(_start, Length).LastCharIndexOf(value._core.GetGraphemeSpan(value._start, value.Length), stringComparison);
    }

    /// <summary>
    /// Reports the zero-based character index of the last occurrence of the specified <paramref name="value"/> in this string.
    /// </summary>
    /// <param name="value">The search string.</param>
    /// <param name="stringComparison">One of the enumeration values to specify the rules for this search.</param>
    /// <returns>
    /// The zero-based index position of <paramref name="value"/> if that string is found, or -1 if it is not. If
    /// <paramref name="value"/> is empty, the return value is 0.
    /// </returns>
    public int LastCharIndexOf(ReadOnlySpan<char> value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return CharLength;
        }

        if (IsEmpty)
        {
            return -1;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = value.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[value.Length] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(value.Length);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            return _core.GetGraphemeSpan(_start, Length).LastCharIndexOf(new GraphemeSpan(value, graphemes, indices, value.Length), stringComparison);
        }
        finally
        {
            if (pooledGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledGraphemesArray);
            }
            if (pooledIndicesArray is not null)
            {
                ArrayPool<int>.Shared.Return(pooledIndicesArray);
            }
        }
    }

    /// <summary>
    /// Reports the zero-based grapheme index of the last occurrence of the specified <paramref name="value"/> in this string.
    /// </summary>
    /// <param name="value">The search string.</param>
    /// <param name="stringComparison">One of the enumeration values to specify the rules for this search.</param>
    /// <returns>
    /// The zero-based index position of <paramref name="value"/> if that string is found, or -1 if it is not. If
    /// <paramref name="value"/> is empty, the return value is 0.
    /// </returns>
    public int LastIndexOf(GraphemeString value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return Length;
        }

        if (IsEmpty)
        {
            return -1;
        }

        return _core.GetGraphemeSpan(_start, Length).LastIndexOf(value._core.GetGraphemeSpan(value._start, value.Length), stringComparison).Index;
    }

    /// <summary>
    /// Reports the zero-based grapheme index of the last occurrence of the specified <paramref name="value"/> in this string.
    /// </summary>
    /// <param name="value">The search string.</param>
    /// <param name="stringComparison">One of the enumeration values to specify the rules for this search.</param>
    /// <returns>
    /// The zero-based index position of <paramref name="value"/> if that string is found, or -1 if it is not. If
    /// <paramref name="value"/> is empty, the return value is 0.
    /// </returns>
    public int LastIndexOf(ReadOnlySpan<char> value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return Length;
        }

        if (IsEmpty)
        {
            return -1;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = value.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[value.Length] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(value.Length);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            return _core.GetGraphemeSpan(_start, Length).LastIndexOf(new GraphemeSpan(value, graphemes, indices, value.Length), stringComparison).Index;
        }
        finally
        {
            if (pooledGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledGraphemesArray);
            }
            if (pooledIndicesArray is not null)
            {
                ArrayPool<int>.Shared.Return(pooledIndicesArray);
            }
        }
    }

    /// <summary>
    /// Determines if this string starts with the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The string to compare.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// <see langword="true"/> if this string starts with the specified <paramref name="value"/>; otherwise,
    /// <see langword="false"/>
    /// </returns>
    public bool StartsWith(GraphemeString value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return true;
        }

        if (IsEmpty)
        {
            return false;
        }

        return _core.GetGraphemeSpan(_start, Length).StartsWith(value._core.GetGraphemeSpan(value._start, value.Length), stringComparison);
    }

    /// <summary>
    /// Determines if this string starts with the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The string to compare.</param>
    /// <param name="stringComparison">One of the enumeration values that determines how two values are compared.</param>
    /// <returns>
    /// <see langword="true"/> if this string starts with the specified <paramref name="value"/>; otherwise,
    /// <see langword="false"/>
    /// </returns>
    public bool StartsWith(ReadOnlySpan<char> value, StringComparison stringComparison)
    {
        if (value.IsEmpty)
        {
            return true;
        }

        if (IsEmpty)
        {
            return false;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = value.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[value.Length] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(value.Length);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            return _core.GetGraphemeSpan(_start, Length).StartsWith(new GraphemeSpan(value, graphemes, indices, value.Length), stringComparison);
        }
        finally
        {
            if (pooledGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledGraphemesArray);
            }
            if (pooledIndicesArray is not null)
            {
                ArrayPool<int>.Shared.Return(pooledIndicesArray);
            }
        }
    }

    /// <inheritdoc/>
    int IComparable<GraphemeString>.CompareTo(GraphemeString? other)
    {
        return CompareTo(other, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    bool IEquatable<GraphemeString>.Equals(GraphemeString? other)
    {
        return Equals(other, StringComparison.Ordinal);
    }
}