using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public abstract class BaseJobCreator : IJobCreator
{
    public abstract Task<Job> CreateJob(
        BaseMessage baseMessage,
        Guid jobGuid,
        bool shouldMarkJobAsHandlingStarted
    );
    
    protected async Task<Job> CreateJobWithinUnitOfWork(
        BaseMessage baseMessage,
        Guid jobGuid, 
        bool shouldMarkJobAsHandlingStarted,
        NhibernateUnitOfWork unitOfWork
    )
    {
        var jobRepository = new JobRepository(unitOfWork);
        var job = await jobRepository.GetByGuidAsync(jobGuid);

        if (job == null)
        {
            var jobKind = baseMessage switch
            {
                Command => JobKind.Command,
                DomainEvent => JobKind.DomainEvent,
                _ => throw new NotSupportedException($"Unsupported BaseMessage type {baseMessage.GetType().FullName}")
            };
            job = new Job(
                jobGuid,
                baseMessage.GetType().Name,
                baseMessage, 
                jobKind,
                baseMessage.RequestId
            );

            if (baseMessage is DomainEvent evnt)
            {
                var relatedCommandJob = await jobRepository.LoadByGuidAsync(evnt.RelatedCommandGuid);
                job.SetRelatedCommandJob(relatedCommandJob);
            }
        
            await jobRepository.SaveAsync(job);
        }

        if (!job.HasCompleted() && shouldMarkJobAsHandlingStarted)
        {
            job.HandlingStarted();
        }

        return job;
    }        
}