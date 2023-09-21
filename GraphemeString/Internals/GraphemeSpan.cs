using System.Runtime.CompilerServices;
using System.Text;

namespace TsukuyoOka.Text.Unicode.Internals;

internal readonly ref struct GraphemeSpan
{
    private readonly ReadOnlySpan<char> _valueSpan;
    private readonly Span<GraphemeCharRange> _graphemes;
    private readonly Span<int> _indices;

    public int Length => _graphemes.Length;
    public int CharLength => _indices.Length;
    public ReadOnlySpan<char> this[Index index] => _valueSpan[GraphemeCoreUtility.GraphemeToRange(_graphemes[index])];
    public ReadOnlySpan<char> this[Range range] => _valueSpan[GraphemeCoreUtility.GraphemesToRange(_graphemes[range])];

    public GraphemeSpan(GraphemeStringCore core, int start, int length)
    {
        _valueSpan = core.GetValueSpan(0, null);
        _graphemes = core.Graphemes.AsSpan(start, length);
        _indices = core.Indices.AsSpan(GraphemeCoreUtility.GraphemesToRange(_graphemes));
    }

    public GraphemeSpan(ReadOnlySpan<char> valueSpan, Span<GraphemeCharRange> graphemes, Span<int> indices, int maxGraphemeLength)
    {
        var (length, charLength) = GraphemeCoreUtility.GetGraphemeInfo(valueSpan, graphemes, indices, maxGraphemeLength);
        _valueSpan = valueSpan[..charLength];
        _graphemes = graphemes[..length];
        _indices = indices[..charLength];
    }

    public GraphemeSpan(GraphemeSpan graphemeSpan, int start, int length)
    {
        _valueSpan = graphemeSpan._valueSpan;
        _graphemes = graphemeSpan._graphemes.Slice(start, length);
        _indices = graphemeSpan._indices[(_graphemes[0].Start - graphemeSpan._graphemes[0].Start)..(_graphemes[^1].End - graphemeSpan._graphemes[0].Start)];
    }

    public int CharIndexOf(GraphemeSpan value, StringComparison stringComparison)
    {
        return IndexOf(value, 0, stringComparison).CharIndex;
    }

    public int LastCharIndexOf(GraphemeSpan value, StringComparison stringComparison)
    {
        return LastIndexOf(value, stringComparison).CharIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int Index, int CharIndex) IndexOf(GraphemeSpan value, int startIndex, StringComparison stringComparison)
    {
        for (var i = startIndex; i < Length;)
        {
            var charStart = _graphemes[i].Start - _graphemes[0].Start;
            var charIndex = this[i..].IndexOf(value[..], stringComparison);
            if (charIndex < 0)
            {
                return (-1, -1);
            }

            var index = _indices[charStart + charIndex];
            if (stringComparison is not StringComparison.Ordinal and not StringComparison.OrdinalIgnoreCase ||
                this[index..(index + value.Length)].Equals(value[..], stringComparison))
            {
                return (index - _indices[0], charStart + charIndex);
            }

            i = index + 1;
        }

        return (-1, -1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int Index, int CharIndex) LastIndexOf(GraphemeSpan value, StringComparison stringComparison)
    {
        for (var i = Length; i > 0;)
        {
            var charIndex = this[..i].LastIndexOf(value[..], stringComparison);
            if (charIndex < 0)
            {
                return (-1, -1);
            }

            var index = _indices[charIndex];
            if (stringComparison is not StringComparison.Ordinal and not StringComparison.OrdinalIgnoreCase ||
                this[index..(index + value.Length)].Equals(value[..], stringComparison))
            {
                return (index - _indices[0], charIndex);
            }

            i -= Length - _indices[charIndex + value.Length - 1] + 1;
        }

        return (-1, -1);
    }

    public bool StartsWith(GraphemeSpan value, StringComparison stringComparison)
    {
        if (stringComparison is not StringComparison.Ordinal and not StringComparison.OrdinalIgnoreCase)
        {
            return this[..].StartsWith(value[..], stringComparison) && this[..].IndexOf(value[..], stringComparison) >= 0;
        }

        return this[..value.Length].Equals(value[..], stringComparison);
    }

    public bool EndsWith(GraphemeSpan value, StringComparison stringComparison)
    {
        if (stringComparison is not StringComparison.Ordinal and not StringComparison.OrdinalIgnoreCase)
        {
            return this[..].EndsWith(value[..], stringComparison) && this[..].LastIndexOf(value[..], stringComparison) >= 0;
        }

        return this[^value.Length..].Equals(value[..], stringComparison);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int Start, int End) TrimWhiteSpaces(TrimType trimType)
    {
        var start = 0;
        if (trimType.HasFlag(TrimType.Start))
        {
            while (start < Length && this[start].IsWhiteSpace())
            {
                start++;
            }

            if (start >= Length)
            {
                return (0, 0);
            }
        }

        var end = Length;
        if (trimType.HasFlag(TrimType.End))
        {
            while (end > start && this[end - 1].IsWhiteSpace())
            {
                end--;
            }

            if (end <= start)
            {
                return (0, 0);
            }
        }

        return (start, end);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int Start, int End) Trim(GraphemeSpan trimSpan, StringComparison stringComparison, TrimType trimType)
    {
        var start = 0;
        if (trimType.HasFlag(TrimType.Start))
        {
            while (start < Length && trimSpan.IndexOf(new GraphemeSpan(this, start, 1), 0, stringComparison).Index >= 0)
            {
                start++;
            }

            if (start >= Length)
            {
                return (0, 0);
            }
        }

        var end = Length;
        if (trimType.HasFlag(TrimType.End))
        {
            while (end > start && trimSpan.IndexOf(new GraphemeSpan(this, end - 1, 1), 0, stringComparison).Index >= 0)
            {
                end--;
            }

            if (end <= start)
            {
                return (0, 0);
            }
        }

        return (start, end);
    }

    public int TruncateByByteLength(Encoding encoding, int maxByteLength)
    {
        if (encoding.Equals(Encoding.Unicode) || encoding.Equals(Encoding.BigEndianUnicode))
        {
            return TruncateByCharLength(maxByteLength / 2);
        }

        if (encoding.Equals(Encoding.UTF32))
        {
            return TruncateByCodePointLength(maxByteLength / 4);
        }

        var length = GetIndex(Math.Min(maxByteLength, CharLength));
        var byteLength = encoding.GetByteCount(this[..length]);
        if (byteLength <= maxByteLength)
        {
            return length;
        }

        var maxByteLengthPerChar = encoding.GetMaxByteCount(2) - encoding.GetMaxByteCount(1);
        var count = 0;
        var start = 0;
        var end = Math.Max(GetIndex(maxByteLength / maxByteLengthPerChar) - 1, 1);
        while (end < Length)
        {
            count += encoding.GetByteCount(this[start..end]);
            if (count >= maxByteLength)
            {
                return count == maxByteLength ? end : end - 1;
            }

            start = end;
            end = Math.Max(GetIndex(GetCharIndex(end) + (maxByteLength - count) / maxByteLengthPerChar) - 1, end + 1);
        }

        return Length - 1;
    }

    public int TruncateByCharLength(int maxCharLength)
    {
        if (maxCharLength == 0)
        {
            return 0;
        }

        if (CharLength <= maxCharLength)
        {
            return Length;
        }

        return GetIndex(maxCharLength);
    }

    public int TruncateByCodePointLength(int maxCodePointLength)
    {
        if (maxCodePointLength == 0)
        {
            return 0;
        }

        if (CharLength <= maxCodePointLength)
        {
            return Length;
        }

        var codePointIndex = 0;
        for (var i = 0; i < CharLength; i++)
        {
            if (i + 1 < CharLength && char.IsHighSurrogate(_valueSpan[i]) && char.IsLowSurrogate(_valueSpan[i + 1]))
            {
                i++;
            }

            codePointIndex++;
            if (codePointIndex > maxCodePointLength)
            {
                return TruncateByCharLength(i);
            }
        }

        return Length;
    }

    public int GetIndex(int charIndex)
    {
        return (charIndex == CharLength ? Length : _indices[charIndex]) - _indices[0];
    }

    public int GetCharIndex(int index)
    {
        return (index == Length ? _graphemes[^1].End : _graphemes[index].Start) - _graphemes[0].Start;
    }
}