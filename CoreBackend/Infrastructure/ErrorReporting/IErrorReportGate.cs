namespace CoreBackend.Infrastructure.ErrorReporting;

public interface IErrorReportGate
{
    Task<bool> TryReserveAsync(ErrorReport report, CancellationToken cancellationToken);
}
