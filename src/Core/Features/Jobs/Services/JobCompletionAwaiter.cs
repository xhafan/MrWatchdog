using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using System.Diagnostics;
using CoreUtils;
using MrWatchdog.Core.Infrastructure.Rebus;

namespace MrWatchdog.Core.Features.Jobs.Services;

public class JobCompletionAwaiter(INhibernateConfigurator nhibernateConfigurator) : IJobCompletionAwaiter
{
    public const int DefaultTimeoutInMilliseconds = 60000;
    
    public async Task WaitForJobCompletion(Guid jobGuid, int timeoutInMilliseconds = DefaultTimeoutInMilliseconds)
    {
        var timer = new Stopwatch();
        timer.Start();
        while (timer.Elapsed.TotalMilliseconds <= timeoutInMilliseconds)
        {
            var hasJobCompleted = await NhibernateUnitOfWorkRunner.RunAsync(
                () => new NhibernateUnitOfWork(nhibernateConfigurator),
                async unitOfWork =>
                {
                    var job = await unitOfWork.Session!.QueryOver<Job>()
                        .Where(x => x.Guid == jobGuid)
                        .SingleOrDefaultAsync();

                    if (job == null)
                    {
                        await Task.Delay(100);
                    }
                    else if (job.NumberOfHandlingAttempts >= RebusConstants.MaxDeliveryAttempts 
                             && !string.IsNullOrWhiteSpace(job.GetLastException()))
                    {
                        throw new Exception($"Job {jobGuid} failed: {job.GetLastException()}");
                    }
                    else if (job.CompletedOn == null)
                    {
                        await Task.Delay(100);
                    }
                    else
                    {
                        return true;
                    }
                    return false;
                }
            );
            if (hasJobCompleted) break;
        }
        timer.Stop();
        Guard.Hope<TimeoutException>(timer.Elapsed.TotalMilliseconds <= timeoutInMilliseconds, $"Job {jobGuid} completion timeout.");
    }
}
