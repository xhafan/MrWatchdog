#pragma warning disable IDE0130
namespace Shouldly;
#pragma warning restore IDE0130

public static class ShouldlyStringExtensions
{
    public static void ShouldBe(
        this string? actual,
        string? expected,
        bool ignoreLineEndings
    )
    {
        if (!ignoreLineEndings)
        {
            actual.ShouldBe(expected);
            return;
        }

        var normalizedActual = actual._NormalizeNewLines();
        var normalizedExpected = expected._NormalizeNewLines();

        normalizedActual.ShouldBe(normalizedExpected);
    }

    private static string? _NormalizeNewLines(this string? s) => s?.Replace("\r\n", "\n");
}
