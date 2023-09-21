# GraphemeString

GraphemeString provides the ability to process .NET strings in units of Unicode grapheme clusters.

## What does it do?

GraphemeString provides the following features:

* Compare and search strings using grapheme clusters as smallest units.
* Trim, substring, and split a string without breaking grapheme clusters.
* Truncate a string to fit a specified grapheme cluster length, code point length, char length,
  and encoded byte length without breaking grapheme clusters.

## What is a grapheme cluster?

Grapheme clusters are rules that define the range of a single character that corresponds to human senses.

In .NET, strings are handled in UTF-16; Unicode originally intended to represent all characters as fixed 2 bytes,
and UTF-16 was created with that in mind. Therefore, the .NET char type is 2 bytes.

However, attempts to represent a character as a fixed 2-byte character have long since been abandoned,
and in the current UTF-16 there are characters encoded to 4 bytes by surrogate pairs,
and even characters encoded to 30 bytes by ligatures. But does anyone really use such strange characters?
A typical example is emoji, which are now used by people all over the world.
The Length property of .NET's string type indicates the size of the char array that the string contains,
which means that if you simply count the number of characters,
the former is 2 characters and the latter is 15 characters.

This is especially a problem when trying to cut a substring.
This is because the fact that a character is treated as multiple characters in the program means
that you can cut it off in the middle of a character. When this happens, the character will be broken.

These problems can be solved by processing by grapheme clusters. If you count the number of grapheme clusters,
you will get the number of characters as they appear in the string, and if you split only at the boundaries,
the characters will not be broken.

## NuGet package

https://www.nuget.org/packages/TsukuyoOka.GraphemeString/

```powershell
Install-Package TsukuyoOka.GraphemeString
```

## Sample code

```C#
using TsukuyoOka.Text.Unicode;

var value = "👨\u200D👨\u200D👦\u200D👧👩\u200D👩\u200D👧\u200D👦";

// Output: 👨‍👨‍👦‍👧👩‍👩‍👧‍👦
Console.WriteLine(value);
// Output: 22
Console.WriteLine(value.Length);
// Output: �
// (\uD83D, i.e. the high surrogate of U+1F468 [👨]
//  This is illegal unless paired with a low surrogate character, which is replaced by '�' in the output)
Console.WriteLine(value[0]);
// Output: �
// (\uDC68, i.e. the low surrogate of U+1F468 [👨]
//  This is illegal unless paired with a high surrogate character, which is replaced by '�' in the output)
Console.WriteLine(value[1]);

var graphemes = value.ToGraphemeString();

// Output: 22
Console.WriteLine(graphemes.CharLength);
// Output: 2
Console.WriteLine(graphemes.Length);
// Output: 👨‍👨‍👦‍👧
Console.WriteLine(graphemes[0].ToString());
// Output: 👩‍👩‍👧‍👦
Console.WriteLine(graphemes[1].ToString());
// Output: 2
Console.WriteLine(value.CountGraphemes());

// These are examples of truncating a string to fit a specified byte length.

// Output: The quick brow
// (A character in the ASCII range is 1 byte even in UTF-8, so 14 bytes would be 14 characters)
Console.WriteLine("The quick brown fox jumps over the lazy dog.".TruncateByByteLength(Encoding.UTF8, 14));
// Output: いろはに
// (Hiragana is 3 bytes per character in UTF-8, so 4 characters is the limit to keep it within 14 bytes)
Console.WriteLine("いろはにほへとちりぬるを".TruncateByByteLength(Encoding.UTF8, 14));
// Output: 
// (The emoji 👨‍👨‍👦‍👧 is as large as 25 bytes in UTF-8, so 14 bytes is not enough)
Console.WriteLine("👨\u200D👨\u200D👦\u200D👧👩\u200D👩\u200D👧\u200D👦".TruncateByByteLength(Encoding.UTF8, 14));
```

## Notes

### Versions

GraphemeString uses the .NET Core API for determining grapheme cluster boundaries.
Therefore, the results may differ depending on the Unicode version supported by .NET.

It appears that .NET 6 supports Unicode 13.0, .NET 7 supports Unicode 14.0, and .NET 8 supports Unicode 15.0.

### Line breaks

CR, LF, and CRLF for line breaks are each treated as a single grapheme cluster.
This means that GraphemeString will not hit CRLF when searching for LF.

### Culture dependencies

If you use a StringComparison enumeration value other than Ordinal or OrdinalIgnoreCase,
the grapheme processing depends on that culture.

The note about line breaks in the previous section also applies only to Ordinal case,
which is otherwise depends on the culture.
