namespace Shouldly;

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
