namespace MrWatchdog.Core.Features.Jobs.Services;

public interface IJobCompletionAwaiter
{
    Task WaitForJobCompletion(Guid jobGuid, int timeoutInMilliseconds = JobCompletionAwaiter.DefaultTimeoutInMilliseconds);
}