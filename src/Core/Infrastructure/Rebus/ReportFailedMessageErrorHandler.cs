using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Jobs;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Messages;
using Rebus.Messages;
using Rebus.Retry;
using Rebus.Serialization;
using Rebus.Transport;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public class ReportFailedMessageErrorHandler(
    IErrorHandler errorHandler,
    ISerializer serializer,
    ICoreBus bus,
    IOptions<RuntimeOptions> iRuntimeOptions,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions
)
: IErrorHandler
{
    public async Task HandlePoisonMessage(
        TransportMessage transportMessage, 
        ITransactionContext transactionContext,
        ExceptionInfo exceptionInfo
    )
    {
        await errorHandler.HandlePoisonMessage(transportMessage, transactionContext, exceptionInfo);

        var rebusMessage = await serializer.Deserialize(transportMessage);

        if (rebusMessage.Body is not BaseMessage message) return;

        var jobGuid = Guid.Parse(rebusMessage.Headers[Headers.MessageId]);
        var failedMessageTypeName = message.GetType().Name;

        await bus.Send(new SendEmailCommand(
            iEmailAddressesOptions.Value.BackendErrors,
            Subject: $"Job {failedMessageTypeName} failed on {iRuntimeOptions.Value.Environment}",
            HtmlMessage: $"""
                          Job <a href="{iRuntimeOptions.Value.Url}{JobUrlConstants.GetJobUrlTemplate.WithJobGuid(jobGuid)}">{failedMessageTypeName}</a> 
                          failed on {iRuntimeOptions.Value.Environment}.
                          """
        ));
    }
}
