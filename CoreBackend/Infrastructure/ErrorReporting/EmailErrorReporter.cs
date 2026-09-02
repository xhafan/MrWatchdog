using System.Net;
using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using Microsoft.Extensions.Options;

namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class EmailErrorReporter(
    ICoreBus bus,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions
) : IErrorReporter
{
    public async Task Report(ErrorReport report, CancellationToken cancellationToken = default)
    {
        await bus.Send(new SendEmailCommand(
            iEmailAddressesOptions.Value.FrontendErrors,
            report.Message[..Math.Min(80, report.Message.Length)],
            $"""
             RequestId: {WebUtility.HtmlEncode(report.RequestId)}<br/>
             TimeStamp: {report.OccurredAtUtc:yyyy-MM-dd HH:mm:ss.fff} UTC<br/>
             Message: {WebUtility.HtmlEncode(report.Message)}
             """
        ));
    }
}
