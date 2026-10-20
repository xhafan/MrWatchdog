using CoreBackend.Infrastructure.ErrorReporting;
using Rebus.Pipeline;

namespace CoreBackend.Infrastructure.Rebus;

public sealed class SuppressErrorReportingDuringMessageHandlingStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        using var suppression = ErrorReportingLogPolicy.Suppress();
        await next();
    }
}
