namespace CoreBackend.Infrastructure.ErrorReporting;

public interface IErrorReporter
{
    Task Report(ErrorReport report, CancellationToken cancellationToken = default);
}
