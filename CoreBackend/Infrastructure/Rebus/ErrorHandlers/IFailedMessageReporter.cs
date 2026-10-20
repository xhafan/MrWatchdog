namespace CoreBackend.Infrastructure.Rebus.ErrorHandlers;

public interface IFailedMessageReporter
{
    Task Report(FailedMessageReport report);
}
