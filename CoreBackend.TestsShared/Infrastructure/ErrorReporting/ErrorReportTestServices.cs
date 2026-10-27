using CoreBackend.Infrastructure.ErrorReporting;
using Microsoft.Extensions.Options;

namespace CoreBackend.TestsShared.Infrastructure.ErrorReporting;

public static class ErrorReportTestServices
{
    public static IErrorReportGate CreateGate(TimeProvider? clock = null, TimeSpan? window = null)
    {
        clock ??= TimeProvider.System;
        return new InMemoryErrorReportGate(
            Options.Create(new ErrorReportingOptions { SuppressionWindow = window ?? TimeSpan.FromDays(1) }), clock, "Test");
    }
}

public sealed class ErrorReportTestClock : TimeProvider
{
    private DateTimeOffset _now = new(2026, 10, 27, 8, 5, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan interval) => _now += interval;
}
