using System.Buffers;
using System.Globalization;

namespace TsukuyoOka.Text.Unicode.Internals;

internal static class GraphemeCoreUtility
{
    public const int MaxStackalloc8 = 16;
    public const int MaxStackalloc4 = 32;

    public static (int Length, int CharLength) GetGraphemeInfo(ReadOnlySpan<char> text, Span<GraphemeCharRange> graphemes, Span<int> indices, int maxGraphemeLength)
    {
        var i = 0;
        var index = 0;
        int next;
        while ((next = StringInfo.GetNextTextElementLength(text[index..])) > 0 && index + next <= text.Length && i < maxGraphemeLength)
        {
            graphemes[i] = new GraphemeCharRange(index, next);
            indices.Slice(index, next).Fill(i);
            index += next;
            i++;
        }

        return (i, index);
    }

    public static (GraphemeCharRange[] graphemes, int[] graphemeIndices) GetGraphemeInfo(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
        {
            return (Array.Empty<GraphemeCharRange>(), Array.Empty<int>());
        }

        GraphemeCharRange[]? pooledGraphemesArray = null;
        try
        {
            var graphemes = text.Length <= MaxStackalloc8 ? stackalloc GraphemeCharRange[text.Length] : pooledGraphemesArray = ArrayPool<GraphemeCharRange>.Shared.Rent(text.Length);
            var indices = new int[text.Length];
            var (length, _) = GetGraphemeInfo(text, graphemes, indices, graphemes.Length);

            return (graphemes[..length].ToArray(), indices);
        }
        finally
        {
            if (pooledGraphemesArray is not null)
            {
                ArrayPool<GraphemeCharRange>.Shared.Return(pooledGraphemesArray);
            }
        }
    }

    public static int GetGraphemeLength(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
        {
            return 0;
        }

        var length = text.Length;
        var i = 0;
        var index = 0;
        int next;
        while ((next = StringInfo.GetNextTextElementLength(text[index..])) > 0 && index + next <= length)
        {
            index += next;
            i++;
        }

        return i;
    }

    public static Range GraphemesToRange(Span<GraphemeCharRange> graphemeCharRanges)
    {
        if (graphemeCharRanges.IsEmpty)
        {
            return ..0;
        }

        return graphemeCharRanges[0].Start..graphemeCharRanges[^1].End;
    }

    public static Range GraphemeToRange(GraphemeCharRange graphemeCharRange)
    {
        return graphemeCharRange.Start..graphemeCharRange.End;
    }
}