namespace CoreBackend.Infrastructure.Rebus.ErrorHandlers;

public interface IFailedMessageReporter
{
    Task Report(Guid jobGuid, Type failedMessageType);
}
