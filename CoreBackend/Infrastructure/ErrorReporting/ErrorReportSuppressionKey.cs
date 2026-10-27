namespace CoreBackend.Infrastructure.ErrorReporting;

public static class ErrorReportSuppressionKey
{
    public static string Create(ErrorReport report, string scope)
    {
        var fingerprint = report.Fingerprint ?? (report.FailedMessage is { } failure
            ? failure.Fingerprint ?? ErrorReportFingerprint.ForFailedMessage(failure.FailedMessageType, failure.ErrorDetails)
            : ErrorReportFingerprint.ForMessage(report.Origin.ToString(), report.Message));
        return "error-report:v1:" + ErrorReportFingerprint.ForMessage(scope + "\0" + report.Origin, fingerprint);
    }
}
