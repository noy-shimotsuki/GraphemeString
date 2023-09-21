using System.Buffers;
using System.Text;
using TsukuyoOka.Text.Unicode.Internals;

namespace TsukuyoOka.Text.Unicode;

/// <summary>
/// Provides extension methods for the string class.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Generates and returns a <see cref="GraphemeString"/> from the current string.
    /// </summary>
    /// <param name="value">The string.</param>
    /// <returns>
    /// A <see cref="GraphemeString"/> instance generated from the current string. If the string is empty, returns
    /// <see cref="GraphemeString.Empty"/>.
    /// </returns>
    public static GraphemeString ToGraphemeString(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return GraphemeString.Empty;
        }

        return new GraphemeString(value);
    }

    /// <summary>
    /// Truncates this string to fit within the specified grapheme cluster length.
    /// </summary>
    /// <param name="value">This string.</param>
    /// <param name="maxGraphemeLength">Maximum grapheme cluster length.</param>
    /// <returns>A string truncated to fit a specified grapheme cluster length.</returns>
    public static string TruncateByGraphemeLength(this string value, int maxGraphemeLength)
    {
        if (maxGraphemeLength < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(maxGraphemeLength));
        }
        else if (maxGraphemeLength == 0)
        {
            return string.Empty;
        }
        else if (value.Length <= maxGraphemeLength)
        {
            return value;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = maxGraphemeLength <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[maxGraphemeLength] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(maxGraphemeLength);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            return new GraphemeSpan(value, graphemes, indices, maxGraphemeLength)[..].ToString();
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
    /// Truncates this string to fit the specified char length without breaking grapheme cluster boundaries.
    /// </summary>
    /// <param name="value">This string.</param>
    /// <param name="maxCharLength">Maximum char length.</param>
    /// <returns>A string truncated to fit a specified char length without breaking grapheme cluster boundaries.</returns>
    public static string TruncateByCharLength(this string value, int maxCharLength)
    {
        if (maxCharLength < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(maxCharLength));
        }
        else if (maxCharLength == 0)
        {
            return string.Empty;
        }
        else if (value.Length <= maxCharLength)
        {
            return value;
        }


        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = maxCharLength <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[maxCharLength] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(maxCharLength);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            var span = new GraphemeSpan(value, graphemes, indices, maxCharLength);
            var end = span.TruncateByCharLength(maxCharLength);
            if (end == 0)
            {
                return string.Empty;
            }

            return span[..end].ToString();
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
    /// Truncates this string to fit the specified code point length without breaking grapheme cluster boundaries.
    /// </summary>
    /// <param name="value">This string.</param>
    /// <param name="maxCodePointLength">Maximum code point length.</param>
    /// <returns>A string truncated to fit a specified code point length without breaking grapheme cluster boundaries.</returns>
    public static string TruncateByCodePointLength(this string value, int maxCodePointLength)
    {
        if (maxCodePointLength < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(maxCodePointLength));
        }
        else if (maxCodePointLength == 0)
        {
            return string.Empty;
        }
        else if (value.Length <= maxCodePointLength)
        {
            return value;
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = maxCodePointLength <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[maxCodePointLength] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(maxCodePointLength);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            var span = new GraphemeSpan(value, graphemes, indices, maxCodePointLength);
            var end = span.TruncateByCodePointLength(maxCodePointLength);
            if (end == 0)
            {
                return string.Empty;
            }

            if (value.Length == span.GetCharIndex(end))
            {
                return value;
            }

            return span[..end].ToString();
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
    /// Truncates this string to fit the specified byte length in the specified encoding without breaking grapheme cluster
    /// boundaries.
    /// </summary>
    /// <param name="value">This string.</param>
    /// <param name="encoding">The encoding used to calculate the byte length.</param>
    /// <param name="maxByteLength">Maximum byte length.</param>
    /// <returns>
    /// A string truncated to fit a specified byte length in the specified encoding without breaking grapheme cluster
    /// boundaries.
    /// </returns>
    public static string TruncateByByteLength(this string value, Encoding encoding, int maxByteLength)
    {
        if (maxByteLength < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(maxByteLength));
        }
        else if (maxByteLength == 0)
        {
            return string.Empty;
        }
        else if (encoding.GetMaxByteCount(value.Length) <= maxByteLength)
        {
            return value;
        }

        var maxGraphemeLength = Math.Min(value.Length, maxByteLength);
        GraphemeCharRange[]? pooledGraphemesArray = null;
        int[]? pooledIndicesArray = null;
        try
        {
            var graphemes = maxGraphemeLength <= GraphemeCoreUtility.MaxStackalloc8 ? stackalloc GraphemeCharRange[maxGraphemeLength] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(maxGraphemeLength);
            var indices = value.Length <= GraphemeCoreUtility.MaxStackalloc4 ? stackalloc int[value.Length] : pooledIndicesArray = ArrayPool<int>.Shared.Rent(value.Length);
            var span = new GraphemeSpan(value, graphemes, indices, maxByteLength);
            var end = span.TruncateByByteLength(encoding, maxByteLength);
            if (end == 0)
            {
                return string.Empty;
            }

            if (value.Length == span.GetCharIndex(end))
            {
                return value;
            }

            return span[..end].ToString();
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
    /// Returns the grapheme cluster length of this string.
    /// </summary>
    /// <param name="value">This string.</param>
    /// <returns>The grapheme cluster length of this string.</returns>
    public static int CountGraphemes(this string value)
    {
        return GraphemeCoreUtility.GetGraphemeLength(value);
    }
}