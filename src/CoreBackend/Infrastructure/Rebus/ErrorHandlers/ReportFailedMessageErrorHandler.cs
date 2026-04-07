using CoreBackend.Messages;
using Rebus.Messages;
using Rebus.Retry;
using Rebus.Serialization;
using Rebus.Transport;

namespace CoreBackend.Infrastructure.Rebus.ErrorHandlers;

public class ReportFailedMessageErrorHandler(
    IErrorHandler errorHandler,
    ISerializer serializer,
    IFailedMessageReporter failedMessageReporter
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

        await failedMessageReporter.Report(jobGuid, message.GetType());
    }
}
