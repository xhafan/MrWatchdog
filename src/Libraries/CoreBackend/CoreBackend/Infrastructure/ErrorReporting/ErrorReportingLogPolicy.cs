using Serilog.Context;
using Serilog.Events;

namespace CoreBackend.Infrastructure.ErrorReporting;

public static class ErrorReportingLogPolicy
{
    public const string SuppressPropertyName = "SuppressErrorReporting";

    public static IDisposable Suppress() => LogContext.PushProperty(SuppressPropertyName, true);

    public static bool ShouldReport(LogEvent logEvent, ErrorReportingOptions options) => logEvent.Level >= LogEventLevel.Error
        && !(logEvent.Properties.TryGetValue(SuppressPropertyName, out var suppressed)
             && suppressed is ScalarValue { Value: true })
        && !_isIgnoredException(logEvent.Exception, options.IgnoredExceptions);

    private static bool _isIgnoredException(Exception? exception, IReadOnlyCollection<Func<Exception, bool>> rules) => exception switch
    {
        null => false,
        AggregateException aggregate => aggregate.InnerExceptions.Count > 0
            && aggregate.InnerExceptions.All(inner => _isIgnoredException(inner, rules)),
        _ => rules.Any(rule => rule(exception)) || _isIgnoredException(exception.InnerException, rules)
    };
}
