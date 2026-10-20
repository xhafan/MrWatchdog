using Serilog;

namespace CoreBackend.Infrastructure.ErrorReporting;

internal static class ErrorReportingDiagnostics
{
    public static void QueueUnavailable() => _logger().Warning(
        "Error report dropped because the delivery queue is full or closed."
    );

    public static void DeliveryFailed(Exception exception) => _logger().Warning(
        "Error report delivery failed. FailureType: {FailureType}", exception.GetType().Name
    );

    public static void ShutdownTimedOut() => _logger().Warning(
        "Error report delivery shutdown timed out; pending reports may be lost."
    );

    private static ILogger _logger() => Log.ForContext(ErrorReportingLogPolicy.SuppressPropertyName, true);
}
