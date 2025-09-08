using MrWatchdog.Core.Messages;
using Rebus.Messages;
using Rebus.Pipeline;
using Serilog.Context;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public class MessageLoggingIncomingStep(ILogger<MessageLoggingIncomingStep> logger) : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var message = context.Load<Message>();

        if (message?.Body is not BaseMessage baseMessage)
        {
            await next();
            return;
        }

        var messageType = baseMessage.GetType().Name;
        var messageId = Guid.Parse(message.Headers[Headers.MessageId]).ToString();
        
        using (LogContext.PushProperty(LogConstants.MessageId, messageId))
        using (LogContext.PushProperty(LogConstants.MessageType, messageType))
        using (LogContext.PushProperty(LogConstants.RequestId, baseMessage.RequestId))
        {
            logger.LogInformation("Message {MessageId} / {MessageType} (RequestId {RequestId}) handling started", messageId, messageType, baseMessage.RequestId);
            try
            {
                await next();

                logger.LogInformation("Message {MessageId} / {MessageType} (RequestId {RequestId}) handled in {Elapsed:0.0000} ms",
                    messageId, messageType, baseMessage.RequestId, stopwatch.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle message {MessageId} / {MessageType} (RequestId {RequestId}) in {Elapsed:0.0000} ms",
                    messageId, messageType, baseMessage.RequestId, stopwatch.Elapsed.TotalMilliseconds);
                throw;
            }
        }
    }
}