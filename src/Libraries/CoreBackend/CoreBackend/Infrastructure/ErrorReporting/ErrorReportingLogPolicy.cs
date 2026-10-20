using Serilog.Context;
using Serilog.Events;

namespace CoreBackend.Infrastructure.ErrorReporting;

public static class ErrorReportingLogPolicy
{
    public const string SuppressPropertyName = "SuppressErrorReporting";

    public static IDisposable Suppress() => LogContext.PushProperty(SuppressPropertyName, true);

    public static bool ShouldReport(LogEvent logEvent) => logEvent.Level >= LogEventLevel.Error
        && !(logEvent.Properties.TryGetValue(SuppressPropertyName, out var suppressed)
             && suppressed is ScalarValue { Value: true });
}
