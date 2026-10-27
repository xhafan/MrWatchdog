using CoreBackend.Messages;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.Infrastructure.EmailSenders;
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
        using var suppression = ErrorReportingLogPolicy.Suppress();
        await errorHandler.HandlePoisonMessage(transportMessage, transactionContext, exceptionInfo);

        transactionContext.OnAck(_ => ReportFailure(transportMessage, exceptionInfo));
    }

    private async Task ReportFailure(TransportMessage transportMessage, ExceptionInfo exceptionInfo)
    {
        using var suppression = ErrorReportingLogPolicy.Suppress();
        try
        {
            var rebusMessage = await serializer.Deserialize(transportMessage);

            if (rebusMessage.Body is not BaseMessage message) return;
            if (message is SendEmailCommand { IsErrorNotification: true }) return;

            await failedMessageReporter.Report(new FailedMessageReport(
                Guid.Parse(rebusMessage.Headers[Headers.MessageId]),
                message.GetType(),
                message.RequestId,
                exceptionInfo.GetFullErrorDescription(),
                DateTime.UtcNow,
                Fingerprint: ErrorReportFingerprint.ForFailedMessage(message.GetType(), exceptionInfo.Details)
            ));
        }
        catch (Exception exception)
        {
            ErrorReportingDiagnostics.DeliveryFailed(exception);
        }
    }
}
