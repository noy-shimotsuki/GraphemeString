using System.Runtime.CompilerServices;

namespace TsukuyoOka.Text.Unicode.Internals;

internal sealed class GraphemeStringCore
{
    private readonly Lazy<(GraphemeCharRange[] Graphemes, int[] Indices)> _info;
    private readonly int _length;
    private readonly string _rawString;
    private readonly int _start;

    public bool IsEmpty => _length == 0;

    public GraphemeCharRange[] Graphemes => _info.Value.Graphemes;
    public int[] Indices => _info.Value.Indices;

    public GraphemeStringCore(string rawString, int start, int length)
    {
        _rawString = rawString;
        _start = start;
        _length = length;
        _info = new Lazy<(GraphemeCharRange[] Graphemes, int[] Indices)>(() => GraphemeCoreUtility.GetGraphemeInfo(rawString.AsSpan(start, length)));
    }

    public GraphemeSpan GetGraphemeSpan(int graphemeIndex, int graphemeLength)
    {
        return new GraphemeSpan(this, graphemeIndex, graphemeLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetValue(int offset, int? length)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        if (offset == 0 && length == null)
        {
            if (_start == 0 && _length == _rawString.Length)
            {
                return _rawString;
            }

            return _rawString[_start..(_start + _length)];
        }
        else if (length is null)
        {
            ThrowHelper.ThrowInvalidOperationException();
        }

        if (Graphemes[offset].Start == 0 && Graphemes[offset + length.Value - 1].End == _rawString.Length)
        {
            return _rawString;
        }

        return GetValueSpan(offset, length.Value).ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetValueSpan(int offset, int? length)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        if (offset == 0 && length is null)
        {
            if (_start == 0 && _length == _rawString.Length)
            {
                return _rawString;
            }

            return _rawString.AsSpan(_start, _length);
        }
        else if (length is null)
        {
            ThrowHelper.ThrowInvalidOperationException();
        }

        if (Graphemes[offset].Start == 0 && Graphemes[offset + length.Value - 1].End == _rawString.Length)
        {
            return _rawString;
        }

        return _rawString.AsSpan(_start, _length)[Graphemes[offset].Start..Graphemes[offset + length.Value - 1].End];
    }

    public ReadOnlySpan<char> GetValueSpan(int graphemeIndex)
    {
        return _rawString.AsSpan(_start, _length)[Graphemes[graphemeIndex].Start..Graphemes[graphemeIndex].End];
    }
}