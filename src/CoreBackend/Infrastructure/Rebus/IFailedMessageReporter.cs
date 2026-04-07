namespace CoreBackend.Infrastructure.Rebus;

public interface IFailedMessageReporter
{
    Task Report(Guid jobGuid, Type failedMessageType);
}
