using Microsoft.Extensions.Logging;

namespace CoreBackend.Infrastructure.ErrorReporting;

public static class ErrorReportingEventIds
{
    public static readonly EventId BrowserError = new(1000, nameof(BrowserError));
}
