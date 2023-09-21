using System.Numerics;
using Xunit.Sdk;

namespace TsukuyoOka.Text.Unicode.Utils;

internal class CompareException : XunitException
{
    public CompareException(string? userMessage) : base(userMessage)
    {
    }

    public CompareException(string? userMessage, Exception? innerException) : base(userMessage, innerException)
    {
    }

    public static CompareException ForNotGreaterThan<T>(string? message, T expect, T actual) where T : INumber<T>
    {
        return new CompareException(
            message ?? $"{nameof(AssertUtil)}.{nameof(AssertUtil.GreaterThan)}() Failure" + Environment.NewLine +
                $"Expected: > {expect}" + Environment.NewLine +
                $"Actual:   {actual}");
    }

    public static CompareException ForNotLowerThan<T>(string? message, T expect, T actual) where T : INumber<T>
    {
        return new CompareException(
            message ?? $"{nameof(AssertUtil)}.{nameof(AssertUtil.LowerThan)}() Failure" + Environment.NewLine +
                $"Expected: < {expect}" + Environment.NewLine +
                $"Actual:   {actual}");
    }

    public static CompareException ForNotLowerThanOrEquals<T>(string? message, T expect, T actual) where T : INumber<T>
    {
        return new CompareException(
            message ?? $"{nameof(AssertUtil)}.{nameof(AssertUtil.LowerThanOrEquals)}() Failure" + Environment.NewLine +
                $"Expected: <= {expect}" + Environment.NewLine +
                $"Actual:   {actual}");
    }
}