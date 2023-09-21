using System.Diagnostics.CodeAnalysis;

namespace TsukuyoOka.Text.Unicode.Internals;

internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowArgumentException(string message, string paramName)
    {
        throw new ArgumentException(message, paramName);
    }

    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException(string paramName)
    {
        throw new ArgumentOutOfRangeException(paramName);
    }

    [DoesNotReturn]
    public static void ThrowInvalidOperationException()
    {
        throw new InvalidOperationException();
    }
}