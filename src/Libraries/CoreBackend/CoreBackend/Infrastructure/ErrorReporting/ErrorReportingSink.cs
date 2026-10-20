using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class ErrorReportingSink(ErrorReportQueue queue) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        if (!ErrorReportingLogPolicy.ShouldReport(logEvent)) return;

        var message = new StringBuilder(logEvent.RenderMessage(CultureInfo.InvariantCulture));
        foreach (var name in new[] { "SourceContext", LogConstants.MessageId, LogConstants.MessageType })
        {
            var value = _property(logEvent, name);
            if (!string.IsNullOrEmpty(value)) message.AppendLine().Append(name).Append(": ").Append(value);
        }
        if (logEvent.Exception != null) message.AppendLine().Append(logEvent.Exception);

        queue.TryEnqueue(new ErrorReport(
            message.ToString(),
            _property(logEvent, LogConstants.RequestId),
            logEvent.Timestamp.UtcDateTime,
            Origin: _isBrowserError(logEvent)
                ? ErrorReportOrigin.Browser
                : ErrorReportOrigin.Server
        ));
    }

    private static bool _isBrowserError(LogEvent logEvent) =>
        logEvent.Properties.TryGetValue("EventId", out var value)
        && value is StructureValue eventId
        && eventId.Properties.Any(property => property.Name == nameof(EventId.Id)
            && property.Value is ScalarValue { Value: int id } && id == ErrorReportingEventIds.BrowserError.Id)
        && eventId.Properties.Any(property => property.Name == nameof(EventId.Name)
            && property.Value is ScalarValue { Value: string name } && name == ErrorReportingEventIds.BrowserError.Name);

    private static string _property(LogEvent logEvent, string name) =>
        logEvent.Properties.TryGetValue(name, out var property) && property is ScalarValue scalar
            ? Convert.ToString(scalar.Value, CultureInfo.InvariantCulture) ?? string.Empty
            : string.Empty;
}
