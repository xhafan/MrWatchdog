﻿using Castle.Windsor;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreUtils;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Messages;
using Rebus.Messages;
using Rebus.Pipeline;
using System.Data;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public class JobTrackingIncomingStep(
    INhibernateConfigurator nhibernateConfigurator,
    ILogger<JobTrackingIncomingStep> logger,
    IWindsorContainer windsorContainer
) : IIncomingStep
{
    private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

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
        
        JobContext.AffectedEntities.Value = [];
        Job? job = null;

        try
        {
            job = await _CreateOrFetchJobInSeparateTransaction(jobGuid, baseMessage);

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

    private async Task<Job> _CreateOrFetchJobInSeparateTransaction(Guid jobGuid, BaseMessage baseMessage)
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(nhibernateConfigurator);
        newUnitOfWork.BeginTransaction(DefaultIsolationLevel);

        var jobRepository = new JobRepository(newUnitOfWork);
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
                jobKind
            );

            if (baseMessage is DomainEvent evnt)
            {
                var relatedCommandJob = await jobRepository.LoadByGuidAsync(evnt.RelatedCommandGuid);
                job.SetRelatedCommandJob(relatedCommandJob);
            }
            
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
            Guard.Hope(JobContext.AffectedEntities.Value != null, $"{nameof(JobContext)} {nameof(JobContext.AffectedEntities)} is null");
            foreach (var affectedEntity in JobContext.AffectedEntities.Value)
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