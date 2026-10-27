using Microsoft.Extensions.Hosting;

namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class ErrorReportDeliveryBackgroundService(ErrorReportQueue queue, IErrorReporter reporter, IErrorReportGate gate) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var report in queue.Reader.ReadAllAsync(stoppingToken))
            {
                using var suppression = ErrorReportingLogPolicy.Suppress();
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                timeout.CancelAfter(queue.Options.DeliveryTimeout);
                try
                {
                    if (!await gate.TryReserveAsync(report, timeout.Token).WaitAsync(timeout.Token)) continue;
                    await reporter.Report(report, timeout.Token).WaitAsync(timeout.Token);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    ErrorReportingDiagnostics.DeliveryFailed(exception);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        queue.Complete();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(queue.Options.ShutdownTimeout);
        try
        {
            if (ExecuteTask != null) await ExecuteTask.WaitAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            ErrorReportingDiagnostics.ShutdownTimedOut();
        }
        finally
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
