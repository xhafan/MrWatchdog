using CoreBackend.Features.Jobs;
using CoreBackend.Infrastructure.Configurations;
using CoreBackend.Infrastructure.EmailSenders;
using Microsoft.Extensions.Options;

namespace CoreBackend.Infrastructure.Rebus.ErrorHandlers;

public class FailedMessageEmailReporter(
    ICoreBus bus,
    IOptions<RuntimeOptions> iRuntimeOptions,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions
) : IFailedMessageReporter
{
    public async Task Report(Guid jobGuid, Type failedMessageType)
    {
        await bus.Send(
            new SendEmailCommand(
                iEmailAddressesOptions.Value.BackendErrors,
                Subject: $"Job {failedMessageType.Name} failed",
                HtmlMessage: $"""
                              Job <a href="{iRuntimeOptions.Value.Url}{JobUrlConstants.GetJobUrlTemplate.WithJobGuid(jobGuid)}">{failedMessageType.Name}</a> failed.
                              """
            )
        );
    }
}
