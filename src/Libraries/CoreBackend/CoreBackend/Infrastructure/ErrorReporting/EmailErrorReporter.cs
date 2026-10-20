using System.Net;
using CoreBackend.Features.Jobs;
using CoreBackend.Infrastructure.Configurations;
using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using Microsoft.Extensions.Options;

namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class EmailErrorReporter(
    ICoreBus bus,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions,
    IOptions<RuntimeOptions> iRuntimeOptions
) : IErrorReporter
{
    public async Task Report(ErrorReport report, CancellationToken cancellationToken = default)
    {
        var failedMessage = report.FailedMessage;
        var jobLink = failedMessage == null
            ? string.Empty
            : $"""
               Job <a href="{WebUtility.HtmlEncode(iRuntimeOptions.Value.Url.TrimEnd('/') + JobUrlConstants.GetJobUrlTemplate.WithJobGuid(failedMessage.JobGuid))}">{WebUtility.HtmlEncode(failedMessage.FailedMessageType.Name)}</a> failed.<br/>
               """;
        await bus.Send(new SendEmailCommand(
            RecipientEmail: report.Origin == ErrorReportOrigin.Browser
                ? iEmailAddressesOptions.Value.FrontendErrors
                : iEmailAddressesOptions.Value.BackendErrors,
            Subject: failedMessage == null
                ? report.Message[..Math.Min(80, report.Message.Length)]
                : $"Job {failedMessage.FailedMessageType.Name} failed",
            HtmlMessage: $"""
                          {jobLink}
                          RequestId: {WebUtility.HtmlEncode(report.RequestId)}<br/>
                          TimeStamp: {report.OccurredAtUtc:yyyy-MM-dd HH:mm:ss.fff} UTC<br/>
                          Message: {WebUtility.HtmlEncode(report.Message)}
                          """,
            IsErrorNotification: true
        ));
    }
}
