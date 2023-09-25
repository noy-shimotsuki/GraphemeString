using System;
using System.Text.Json.Serialization;
using TsukuyoOka.Text.Unicode.Internals;

namespace TsukuyoOka.Text.Unicode;

/// <summary>
/// Provides grapheme-based string processing. This class is immutable.
/// </summary>
[JsonConverter(typeof(GraphemeStringJsonConverter))]
public sealed partial class GraphemeString
{
    private readonly GraphemeStringCore _core;
    private readonly int? _length;
    private readonly int _start;

    /// <summary>
    /// Represents the empty <see cref="GraphemeString"/>.
    /// </summary>
    public static GraphemeString Empty { get; } = new(string.Empty);

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    /// <value><see langword="true"/> if this string is empty; otherwise, <see langword="false"/>.</value>
    public bool IsEmpty => _core.IsEmpty;

    /// <summary>
    /// The number of characters in this instance.
    /// </summary>
    public int CharLength => ValueSpan.Length;

    /// <summary>
    /// The number of graphemes in this instance.
    /// </summary>
    public int Length => _length ?? _core.Graphemes.Length;

    /// <summary>
    /// Gets the string span represented by this instance.
    /// </summary>
    public ReadOnlySpan<char> ValueSpan => _core.GetValueSpan(_start, _length);

    /// <summary>
    /// Gets the grapheme span at a specified range in this instance.
    /// </summary>
    /// <param name="range">A grapheme-based range in this instance.</param>
    /// <returns>The grapheme span at position range.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public GraphemeString this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Length);

            if (length == 0)
            {
                return Empty;
            }

            if (start == 0 && length == Length)
            {
                return this;
            }

            return new GraphemeString(_core, _start + start, length);
        }
    }

    /// <summary>
    /// Gets the grapheme span at a specified position in this instance.
    /// </summary>
    /// <param name="index">A grapheme-based position in this instance.</param>
    /// <returns>The grapheme span at position index.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ReadOnlySpan<char> this[Index index]
    {
        get
        {
            var offset = index.GetOffset(Length);
            if (offset >= Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
            }

            return _core.GetValueSpan(_start + offset);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphemeString"/> structure to the value indicated by a specified
    /// <see langword="string"/>.
    /// </summary>
    /// <param name="baseString">The string.</param>
    public GraphemeString(string baseString)
    {
        _core = new GraphemeStringCore(baseString, 0, baseString.Length);
        _start = 0;
        _length = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphemeString"/> structure to the value indicated by a specified
    /// <see langword="string"/>,
    /// a starting character position within that string, and a length.
    /// </summary>
    /// <param name="baseString">The string.</param>
    /// <param name="startCharIndex">The starting character position within value.</param>
    /// <param name="charLength">The number of characters within value to use.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public GraphemeString(string baseString, int startCharIndex, int charLength)
    {
        if (startCharIndex < 0 || startCharIndex > baseString.Length)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(startCharIndex));
        }

        if (charLength < 0 || startCharIndex + charLength > baseString.Length)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(charLength));
        }

        _core = new GraphemeStringCore(baseString, startCharIndex, charLength);
        _start = 0;
        _length = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphemeString"/> structure to the value indicated by a specified
    /// <see langword="string"/>, and a range within that string.
    /// </summary>
    /// <param name="baseString">The string.</param>
    /// <param name="charRange">The range of characters within value to use.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public GraphemeString(string baseString, Range charRange)
    {
        var (start, length) = charRange.GetOffsetAndLength(baseString.Length);

        _core = new GraphemeStringCore(baseString, start, length);
        _start = 0;
        _length = null;
    }

    private GraphemeString(GraphemeStringCore core, int start, int length)
    {
        _core = core;
        _start = start;
        _length = length;
    }

    /// <summary>
    /// Returns the index of the first char in the specified grapheme cluster.
    /// If a grapheme cluster length is specified, return the char length.
    /// </summary>
    /// <param name="index">The grapheme index.</param>
    /// <returns>The zero-based index position of the first char in the specified grapheme cluster.</returns>
    public int GetCharIndexByIndex(int index)
    {
        if (index == 0)
        {
            return 0;
        }
        if (index < 0 || index > Length)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
        }

        return _core.GetGraphemeSpan(_start, Length).GetCharIndex(index);
    }

    /// <summary>
    /// Retrieves an object that can iterate through the individual graphemes in this instance.
    /// </summary>
    /// <returns></returns>
    public GraphemeEnumerator GetEnumerator()
    {
        return new GraphemeEnumerator(this);
    }

    /// <summary>
    /// Returns the index of the grapheme cluster containing the specified char.
    /// If a char length is specified, return the grapheme cluster length.
    /// </summary>
    /// <param name="charIndex">The char index.</param>
    /// <returns>The zero-based index position of the grapheme cluster containing the specified char.</returns>
    public int GetIndexByCharIndex(int charIndex)
    {
        if (charIndex == 0)
        {
            return 0;
        }
        if (charIndex < 0 || charIndex > CharLength)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(charIndex));
        }

        return _core.GetGraphemeSpan(_start, Length).GetIndex(charIndex);
    }

    /// <summary>
    /// Indicates whether this instance contains only whitespace characters.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if this instance is empty or contains only whitespace characters; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool IsWhiteSpace()
    {
        return ValueSpan.IsWhiteSpace();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _core.GetValue(_start, _length);
    }

    /// <summary>
    /// Supports iterating over a <see cref="GraphemeString"/> object and reading its individual graphemes.
    /// </summary>
    public struct GraphemeEnumerator
    {
        private readonly GraphemeString _graphemeString;
        private readonly int _length;
        private int _index;

        /// <summary>
        /// Gets the currently referenced grapheme in the <see cref="GraphemeString"/> enumerated by this
        /// <see cref="GraphemeEnumerator"/> object.
        /// </summary>
        public readonly ReadOnlySpan<char> Current
        {
            get
            {
                if ((uint)_index >= (uint)_length)
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }

                return _graphemeString[_index];
            }
        }

        internal GraphemeEnumerator(GraphemeString graphemeString)
        {
            _graphemeString = graphemeString;
            _length = graphemeString.Length;
            _index = -1;
        }

        /// <summary>
        /// Increments the internal index of the current <see cref="GraphemeEnumerator"/> object to the next grapheme of the
        /// enumerated <see cref="GraphemeString"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the index is successfully incremented and within the enumerated
        /// <see cref="GraphemeString"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool MoveNext()
        {
            return (uint)++_index < (uint)_length;
        }

        /// <summary>
        /// Initializes the index to a position logically before the first grapheme of the enumerated <see cref="GraphemeString"/>.
        /// </summary>
        public void Reset()
        {
            _index = -1;
        }
    }
}