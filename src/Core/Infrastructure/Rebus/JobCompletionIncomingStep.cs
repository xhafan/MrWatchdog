using CoreUtils;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Messages;
using Rebus.Messages;
using Rebus.Pipeline;

namespace MrWatchdog.Core.Infrastructure.Rebus;

/// <summary>
/// Completes the job in the main unit of work transaction.
/// This step is executed within <see cref="JobTrackingIncomingStep" />
/// </summary>
public class JobCompletionIncomingStep(
    IJobRepositoryFactory jobRepositoryFactory
) : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        await next();

        var rebusMessage = context.Load<Message>();
        if (rebusMessage.Body is not BaseMessage)
        {
            return;
        }

        var jobGuid = Guid.Parse(rebusMessage.Headers[Headers.MessageId]);
        var jobRepository = jobRepositoryFactory.Create();
        var job = await jobRepository.LoadByGuidAsync(jobGuid);

        _MarkJobAsComplete(job);            
    }

    private void _MarkJobAsComplete(Job job)
    {
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
}