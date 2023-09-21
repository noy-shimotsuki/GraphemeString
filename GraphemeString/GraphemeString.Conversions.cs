using System.Buffers;
using System.Text;
using TsukuyoOka.Text.Unicode.Internals;

namespace TsukuyoOka.Text.Unicode;

public partial class GraphemeString
{
    /// <summary>
    /// Returns a new string with all occurrences of <paramref name="oldValue"/> in this string replaced by
    /// <paramref name="newValue"/>.
    /// </summary>
    /// <param name="oldValue">The value to be replaced</param>
    /// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/></param>
    /// <param name="stringComparison">
    /// One of the enumeration values that determines how <paramref name="oldValue"/> is
    /// compared to this instance.
    /// </param>
    /// <returns>
    /// A new string in which all occurrences of <paramref name="oldValue"/> in this string are replaced by
    /// <paramref name="newValue"/>.
    /// If <paramref name="oldValue"/> is not found in the current instance, the method returns the current instance as is.
    /// </returns>
    public GraphemeString Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue, StringComparison stringComparison)
    {
        if (oldValue.IsEmpty)
        {
            ThrowHelper.ThrowArgumentException("String cannot be of zero length.", nameof(oldValue));
        }
        else if (oldValue.Equals(newValue, StringComparison.Ordinal))
        {
            return this;
        }

        GraphemeCharRange[]? pooledOldValueGraphemesArray = null;
        int[]? pooledOldValueIndicesArray = null;
        int[]? pooledPositions = null;
        try
        {
            var oldValueGraphemes = oldValue.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[oldValue.Length] : pooledOldValueGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(oldValue.Length);
            var oldValueIndices = oldValue.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[oldValue.Length] : pooledOldValueIndicesArray = ArrayPool<int>.Shared.Rent(oldValue.Length);
            var oldValueGraphemeSpan = new GraphemeSpan(oldValue, oldValueGraphemes, oldValueIndices, oldValue.Length);
            if (Length < oldValueGraphemeSpan.Length)
            {
                return this;
            }

            var span = _core.GetGraphemeSpan(_start, Length);
            var startIndex = 0;
            var next = 0;
            var positions = Length / oldValueGraphemeSpan.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[Length / oldValueGraphemeSpan.Length] : pooledPositions = ArrayPool<int>.Shared.Rent(Length / oldValueGraphemeSpan.Length);
            var i = 0;
            while ((next = span.IndexOf(oldValueGraphemeSpan, startIndex, stringComparison).Index) >= 0)
            {
                positions[i++] = span.GetCharIndex(next - startIndex);
                startIndex = next + oldValueGraphemeSpan.Length;
            }

            if (i == 0)
            {
                return this;
            }

            var sb = new StringBuilder(CharLength + (newValue.Length - oldValue.Length) * i);
            var index = 0;
            var baseSpan = ValueSpan;
            for (var j = 0; j < i; j++)
            {
                sb.Append(baseSpan.Slice(index, positions[j])).Append(newValue);
                index += positions[j] + oldValue.Length;
            }

            sb.Append(baseSpan[index..]);
            if (sb.Length == 0)
            {
                return Empty;
            }

            return new GraphemeString(sb.ToString());
        }
        finally
        {
            if (pooledOldValueGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledOldValueGraphemesArray);
            }

            if (pooledOldValueIndicesArray is not null)
            {
                ArrayPool<int>.Shared.Return(pooledOldValueIndicesArray);
            }

            if (pooledPositions is not null)
            {
                ArrayPool<int>.Shared.Return(pooledPositions);
            }
        }
    }

    /// <summary>
    /// Splits this string by the specified <paramref name="separator"/> and returns an array of the substrings.
    /// </summary>
    /// <param name="separator">Separator string to split this string.</param>
    /// <param name="stringComparison">
    /// One of the enumeration values that determines how <paramref name="separator"/> is
    /// compared to this instance.
    /// </param>
    /// <param name="stringSplitOptions">
    /// A bitwise combination of the enumeration values specifying whether to trim whitespace
    /// in substrings and to exclude empty substrings.
    /// </param>
    /// <returns>An array of substrings from this instance, delimited by the <paramref name="separator"/>.</returns>
    public GraphemeString[] Split(ReadOnlySpan<char> separator, StringComparison stringComparison, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
    {
        return SplitCore(separator, stringComparison, int.MaxValue, stringSplitOptions);
    }

    /// <summary>
    /// Splits this string by the specified <paramref name="separator"/> and returns an array of the substrings.
    /// </summary>
    /// <param name="separator">Separator string to split this string.</param>
    /// <param name="stringComparison">
    /// One of the enumeration values that determines how <paramref name="separator"/> is
    /// compared to this instance.
    /// </param>
    /// <param name="count">
    /// The maximum number of elements expected in the array. Any <paramref name="separator"/> exceeding
    /// this number will not be delimited, even if it exists.
    /// </param>
    /// <param name="stringSplitOptions">
    /// A bitwise combination of the enumeration values specifying whether to trim whitespace
    /// in substrings and to exclude empty substrings.
    /// </param>
    /// <returns>
    /// An array of substrings from this instance, delimited by the <paramref name="separator"/> into at most
    /// <paramref name="count"/>.
    /// </returns>
    public GraphemeString[] Split(ReadOnlySpan<char> separator, StringComparison stringComparison, int count, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
    {
        return SplitCore(separator, stringComparison, count, stringSplitOptions);
    }

    /// <summary>
    /// Removes all leading and trailing whitespace graphemes from this string.
    /// </summary>
    /// <returns>The string that remains after all whitespace graphemes are removed from the start and end of this string.</returns>
    public GraphemeString Trim()
    {
        if (IsEmpty)
        {
            return this;
        }

        if (!char.IsWhiteSpace(ValueSpan[0]) && !char.IsWhiteSpace(ValueSpan[^1]))
        {
            return this;
        }

        var (start, end) = _core.GetGraphemeSpan(_start, Length).TrimWhiteSpaces(TrimType.Both);

        if (start == end)
        {
            return Empty;
        }

        return new GraphemeString(_core, _start + start, end - start);
    }

    /// <summary>
    /// Removes all leading and trailing specified graphemes from this string.
    /// </summary>
    /// <param name="trimGraphemes">Graphemes to remove</param>
    /// <param name="stringComparison">
    /// One of the enumeration values that determines how <paramref name="trimGraphemes"/> is
    /// compared to this instance.
    /// </param>
    /// <returns>The string that remains after all specified graphemes are removed from the start and end of this string.</returns>
    public GraphemeString Trim(ReadOnlySpan<char> trimGraphemes, StringComparison stringComparison)
    {
        return TrimGraphemesCore(trimGraphemes, stringComparison, TrimType.Both);
    }

    /// <summary>
    /// Removes all trailing whitespace graphemes from this string.
    /// </summary>
    /// <returns>The string that remains after all whitespace graphemes are removed from the end of this string.</returns>
    public GraphemeString TrimEnd()
    {
        if (IsEmpty)
        {
            return this;
        }

        if (!char.IsWhiteSpace(ValueSpan[^1]))
        {
            return this;
        }

        var (start, end) = _core.GetGraphemeSpan(_start, Length).TrimWhiteSpaces(TrimType.End);

        if (start == end)
        {
            return Empty;
        }

        return new GraphemeString(_core, _start + start, end - start);
    }

    /// <summary>
    /// Removes all trailing specified graphemes from this string.
    /// </summary>
    /// <param name="trimGraphemes">Graphemes to remove</param>
    /// <param name="stringComparison">
    /// One of the enumeration values that determines how <paramref name="trimGraphemes"/> is
    /// compared to this instance.
    /// </param>
    /// <returns>The string that remains after all specified graphemes are removed from the start and end of this string.</returns>
    public GraphemeString TrimEnd(ReadOnlySpan<char> trimGraphemes, StringComparison stringComparison)
    {
        return TrimGraphemesCore(trimGraphemes, stringComparison, TrimType.End);
    }

    /// <summary>
    /// Removes all leading whitespace graphemes from this string.
    /// </summary>
    /// <returns>The string that remains after all whitespace graphemes are removed from the start of this string.</returns>
    public GraphemeString TrimStart()
    {
        if (IsEmpty)
        {
            return this;
        }

        if (!char.IsWhiteSpace(ValueSpan[0]))
        {
            return this;
        }

        var (start, end) = _core.GetGraphemeSpan(_start, Length).TrimWhiteSpaces(TrimType.Start);

        if (start == end)
        {
            return Empty;
        }

        return new GraphemeString(_core, _start + start, end - start);
    }

    /// <summary>
    /// Removes all leading specified graphemes from this string.
    /// </summary>
    /// <param name="trimGraphemes">Graphemes to remove</param>
    /// <param name="stringComparison">
    /// One of the enumeration values that determines how <paramref name="trimGraphemes"/> is
    /// compared to this instance.
    /// </param>
    /// <returns>The string that remains after all specified graphemes are removed from the start and end of this string.</returns>
    public GraphemeString TrimStart(ReadOnlySpan<char> trimGraphemes, StringComparison stringComparison)
    {
        return TrimGraphemesCore(trimGraphemes, stringComparison, TrimType.Start);
    }

    /// <summary>
    /// Truncates this string to fit the specified byte length in the specified encoding without breaking grapheme cluster
    /// boundaries.
    /// </summary>
    /// <param name="encoding">The encoding used to calculate the byte length.</param>
    /// <param name="maxByteLength">Maximum byte length.</param>
    /// <returns>
    /// A string truncated to fit a specified byte length in the specified encoding without breaking grapheme cluster
    /// boundaries.
    /// </returns>
    public GraphemeString TruncateByByteLength(Encoding encoding, int maxByteLength)
    {
        if (maxByteLength < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(maxByteLength));
        }
        else if (maxByteLength == 0)
        {
            return Empty;
        }
        else if (encoding.GetMaxByteCount(CharLength) <= maxByteLength)
        {
            return this;
        }

        var end = _core.GetGraphemeSpan(_start, Length).TruncateByByteLength(encoding, maxByteLength);
        if (end == 0)
        {
            return Empty;
        }

        if (end >= Length)
        {
            return this;
        }

        return this[..end];
    }

    /// <summary>
    /// Truncates this string to fit the specified char length without breaking grapheme cluster boundaries.
    /// </summary>
    /// <param name="maxCharLength">Maximum char length.</param>
    /// <returns>A string truncated to fit a specified char length without breaking grapheme cluster boundaries.</returns>
    public GraphemeString TruncateByCharLength(int maxCharLength)
    {
        if (maxCharLength < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(maxCharLength));
        }
        else if (maxCharLength == 0)
        {
            return Empty;
        }
        else if (CharLength <= maxCharLength)
        {
            return this;
        }

        var end = _core.GetGraphemeSpan(_start, Length).TruncateByCharLength(maxCharLength);
        if (end == 0)
        {
            return Empty;
        }

        return this[..end];
    }

    /// <summary>
    /// Truncates this string to fit the specified code point length without breaking grapheme cluster boundaries.
    /// </summary>
    /// <param name="maxCodePointLength">Maximum code point length.</param>
    /// <returns>A string truncated to fit a specified code point length without breaking grapheme cluster boundaries.</returns>
    public GraphemeString TruncateByCodePointLength(int maxCodePointLength)
    {
        if (maxCodePointLength < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(maxCodePointLength));
        }
        else if (maxCodePointLength == 0)
        {
            return Empty;
        }
        else if (CharLength <= maxCodePointLength)
        {
            return this;
        }

        var end = _core.GetGraphemeSpan(_start, Length).TruncateByCodePointLength(maxCodePointLength);
        if (end == 0)
        {
            return Empty;
        }

        if (end >= Length)
        {
            return this;
        }

        return this[..end];
    }

    /// <summary>
    /// Truncates this string to fit within the specified grapheme cluster length.
    /// </summary>
    /// <param name="maxGraphemeLength">Maximum grapheme cluster length.</param>
    /// <returns>A string truncated to fit a specified grapheme cluster length.</returns>
    public GraphemeString TruncateByGraphemeLength(int maxGraphemeLength)
    {
        if (maxGraphemeLength < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(maxGraphemeLength));
        }
        else if (maxGraphemeLength == 0)
        {
            return Empty;
        }
        else if (CharLength <= maxGraphemeLength)
        {
            return this;
        }
        else if (Length <= maxGraphemeLength)
        {
            return this;
        }

        return this[..maxGraphemeLength];
    }

    private GraphemeString[] SplitCore(ReadOnlySpan<char> separator, StringComparison stringComparison, int count, StringSplitOptions stringSplitOptions)
    {
        if (count < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(count));
        }
        else if (count == 0)
        {
            return Array.Empty<GraphemeString>();
        }
        else if (count == 1 || separator.IsEmpty)
        {
            var result = stringSplitOptions.HasFlag(StringSplitOptions.TrimEntries) ? Trim() : this;
            if (result.IsEmpty && stringSplitOptions.HasFlag(StringSplitOptions.RemoveEmptyEntries))
            {
                return Array.Empty<GraphemeString>();
            }

            return new[] { result };
        }

        var tempSize = Math.Min(count, Length);
        var splitted = ArrayPool<GraphemeString>.Shared.Rent(tempSize);
        GraphemeCharRange[]? pooledSeparatorGraphemesArray = null;
        int[]? pooledSeparatorIndicesArray = null;
        try
        {
            var index = 0;
            var graphemeSpan = _core.GetGraphemeSpan(_start, Length);

            var separatorGraphemes = separator.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[separator.Length] : pooledSeparatorGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(separator.Length);
            var separatorIndices = separator.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[separator.Length] : pooledSeparatorIndicesArray = ArrayPool<int>.Shared.Rent(separator.Length);
            var separatorGraphemeSpan = new GraphemeSpan(separator, separatorGraphemes, separatorIndices, separator.Length);

            var i = 0;
            while (i + 1 < tempSize)
            {
                var end = graphemeSpan.IndexOf(separatorGraphemeSpan, index, stringComparison).Index;
                if (end < 0)
                {
                    break;
                }

                if (index == end)
                {
                    if (!stringSplitOptions.HasFlag(StringSplitOptions.RemoveEmptyEntries))
                    {
                        splitted[i++] = Empty;
                    }
                }
                else if (stringSplitOptions.HasFlag(StringSplitOptions.TrimEntries))
                {
                    var (trimmedStart, trimmedEnd) = new GraphemeSpan(_core, _start + index, end - index).TrimWhiteSpaces(TrimType.Both);
                    if (trimmedStart == trimmedEnd)
                    {
                        if (!stringSplitOptions.HasFlag(StringSplitOptions.RemoveEmptyEntries))
                        {
                            splitted[i++] = Empty;
                        }
                    }
                    else
                    {
                        splitted[i++] = new GraphemeString(_core, _start + index + trimmedStart, trimmedEnd - trimmedStart);
                    }
                }
                else
                {
                    splitted[i++] = new GraphemeString(_core, _start + index, end - index);
                }

                index = end + separatorGraphemeSpan.Length;
            }

            if (stringSplitOptions.HasFlag(StringSplitOptions.TrimEntries))
            {
                var (trimmedStart, trimmedEnd) = new GraphemeSpan(_core, _start + index, Length - index).TrimWhiteSpaces(TrimType.Both);
                if (trimmedStart == trimmedEnd)
                {
                    if (!stringSplitOptions.HasFlag(StringSplitOptions.RemoveEmptyEntries))
                    {
                        splitted[i++] = Empty;
                    }
                }
                else
                {
                    splitted[i++] = new GraphemeString(_core, _start + index + trimmedStart, trimmedEnd - trimmedStart);
                }
            }
            else if (index < Length || !stringSplitOptions.HasFlag(StringSplitOptions.RemoveEmptyEntries))
            {
                splitted[i++] = new GraphemeString(_core, _start + index, Length - index);
            }

            return splitted[..i];
        }
        finally
        {
            ArrayPool<GraphemeString>.Shared.Return(splitted);

            if (pooledSeparatorGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledSeparatorGraphemesArray);
            }

            if (pooledSeparatorIndicesArray is not null)
            {
                ArrayPool<int>.Shared.Return(pooledSeparatorIndicesArray);
            }
        }
    }

    private GraphemeString TrimGraphemesCore(ReadOnlySpan<char> trimGraphemes, StringComparison stringComparison, TrimType trimType)
    {
        if (IsEmpty || trimGraphemes.IsEmpty)
        {
            return this;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = trimGraphemes.Length <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[trimGraphemes.Length] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(trimGraphemes.Length);
            var indices = trimGraphemes.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[trimGraphemes.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(trimGraphemes.Length);
            var (start, end) = _core.GetGraphemeSpan(_start, Length).Trim(new GraphemeSpan(trimGraphemes, graphemes, indices, graphemes.Length), stringComparison, trimType);
            if (start == 0 && end == Length)
            {
                return this;
            }
            else if (start == end)
            {
                return Empty;
            }

            return new GraphemeString(_core, _start + start, end - start);
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
}