using System.Numerics;

namespace TsukuyoOka.Text.Unicode.Utils;

internal static class AssertUtil
{
    public static void GreaterThan<T>(T expect, T actual, string? message = null) where T : INumber<T>
    {
        if (expect.CompareTo(actual) >= 0)
        {
            throw CompareException.ForNotGreaterThan(message, expect, actual);
        }
    }

    public static void LowerThan<T>(T expect, T actual, string? message = null) where T : INumber<T>
    {
        if (expect.CompareTo(actual) <= 0)
        {
            throw CompareException.ForNotGreaterThan(message, expect, actual);
        }
    }

    public static void LowerThanOrEquals<T>(T expect, T actual, string? message = null) where T : INumber<T>
    {
        if (expect.CompareTo(actual) < 0)
        {
            throw CompareException.ForNotLowerThanOrEquals(message, expect, actual);
        }
    }
}