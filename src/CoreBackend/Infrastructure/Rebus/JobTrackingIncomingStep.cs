using Castle.Windsor;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Messages;
using Rebus.Messages;
using Rebus.Pipeline;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public class JobTrackingIncomingStep(
    INhibernateConfigurator nhibernateConfigurator,
    ILogger<JobTrackingIncomingStep> logger,
    IWindsorContainer windsorContainer,
    IJobCreator jobCreator,
    IRebusHandlingQueueGetter rebusHandlingQueueGetter
) : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        JobContext.WindsorContainer.Value = windsorContainer;
        JobContext.RaisedDomainEvents.Value = [];

        var rebusMessage = context.Load<Message>();

        if (rebusMessage.Body is not BaseMessage baseMessage)
        {
            await next();
            return;
        }

        var jobGuid = Guid.Parse(rebusMessage.Headers[Headers.MessageId]);

        JobContext.RebusHandlingQueue.Value = rebusHandlingQueueGetter.GetHandlingQueue();

        switch (baseMessage)
        {
            case Command commandMessage:
                JobContext.CommandGuid.Value = commandMessage.Guid;
                break;
            case DomainEvent domainEventMessage:
                JobContext.CommandGuid.Value = domainEventMessage.RelatedCommandGuid;
                break;
            default:
                throw new NotSupportedException($"Unsupported BaseMessage type {baseMessage.GetType().FullName}");
        }
        
        JobContext.ActingUserId.Value = baseMessage.ActingUserId;
        JobContext.RequestId.Value = baseMessage.RequestId;
        Serilog.Context.LogContext.PushProperty(LogConstants.RequestId, baseMessage.RequestId);
        Serilog.Context.LogContext.PushProperty(LogConstants.UserId, baseMessage.ActingUserId);
        
        JobContext.AffectedEntities.Value = [];
        Job? job = null;

        try
        {
            job = await _CreateOrFetchJobInSeparateTransaction(jobGuid, baseMessage);
            if (job.HasCompleted())
            {
                return;
            }
            
            await next();

            // Job has been completed by JobCompletionIncomingStep within the main unit of work transaction.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            if (job != null)
            {
                await _MarkJobAsFailedInSeparateTransaction(job.Id, ex);
            }

            throw;
        }   
    }

    private async Task<Job> _CreateOrFetchJobInSeparateTransaction(Guid jobGuid, BaseMessage baseMessage)
    {
        return await jobCreator.CreateJob(baseMessage, jobGuid, shouldMarkJobAsHandlingStarted: true, JobContext.RebusHandlingQueue.Value);
    }
    
    private async Task _MarkJobAsFailedInSeparateTransaction(long jobId, Exception ex)
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(nhibernateConfigurator),
            async newUnitOfWork =>
            {
                var job = await new JobRepository(newUnitOfWork).LoadByIdAsync(jobId);
                job.Fail(ex);
            }
        );
    }
}