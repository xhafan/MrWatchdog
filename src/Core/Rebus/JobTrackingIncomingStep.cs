using System.Data;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreUtils;
using CoreUtils.AmbientStorages;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Messages;
using Rebus.Messages;
using Rebus.Pipeline;

namespace MrWatchdog.Core.Rebus;

public class JobTrackingIncomingStep(
    INhibernateConfigurator nhibernateConfigurator,
    ILogger<JobTrackingIncomingStep> logger
) : IIncomingStep
{
    private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

    public static readonly AmbientStorage<List<(string EntityName, long EntityId, bool IsCreated)>?> AffectedEntities = new();

    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var rebusMessage = context.Load<Message>();

        if (rebusMessage.Body is not BaseMessage baseMessage)
        {
            await next();
            return;
        }
        
        AffectedEntities.Value = [];
        Job? job = null;

        try
        {
            job = await _CreateOrFetchJobInSeparateTransaction(baseMessage);

            await next();
            
            await _MarkJobAsCompleteInSeparateTransaction(job.Id);            
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

    private async Task<Job> _CreateOrFetchJobInSeparateTransaction(BaseMessage baseMessage)
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(nhibernateConfigurator);
        newUnitOfWork.BeginTransaction(DefaultIsolationLevel);

        var jobRepository = new JobRepository(newUnitOfWork);
        var job = await jobRepository.GetByGuidAsync(baseMessage.Guid);

        if (job == null)
        {
            var jobKind = baseMessage switch
            {
                Command => JobKind.Command,
                DomainEvent => JobKind.DomainEvent,
                _ => throw new NotSupportedException($"Unsupported BaseMessage type {baseMessage.GetType().FullName}")
            };
            job = new Job(
                baseMessage.Guid,
                baseMessage.GetType().Name,
                baseMessage, 
                jobKind
            );
            await jobRepository.SaveAsync(job);
        }
        
        job.HandlingStarted();

        return job;
    }
    
    private async Task _MarkJobAsCompleteInSeparateTransaction(long jobId)
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(nhibernateConfigurator);
        newUnitOfWork.BeginTransaction(DefaultIsolationLevel);
        
        var job = await new NhibernateRepository<Job>(newUnitOfWork).LoadByIdAsync(jobId);

        job.Complete();

        _addAffectedEntitiesToJob();
        return;

        void _addAffectedEntitiesToJob()
        {
            Guard.Hope(AffectedEntities.Value != null, "AffectedEntities.Value" + " is null");
            foreach (var affectedEntity in AffectedEntities.Value)
            {
                job.AddAffectedEntity(
                    affectedEntity.EntityName,
                    affectedEntity.EntityId,
                    affectedEntity.IsCreated
                );
            }
        }
    }   
    
    private async Task _MarkJobAsFailedInSeparateTransaction(long jobId, Exception ex)
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(nhibernateConfigurator);
        newUnitOfWork.BeginTransaction(DefaultIsolationLevel);
        
        var job = await new NhibernateRepository<Job>(newUnitOfWork).LoadByIdAsync(jobId);

        job.Fail(ex);
    } 
}