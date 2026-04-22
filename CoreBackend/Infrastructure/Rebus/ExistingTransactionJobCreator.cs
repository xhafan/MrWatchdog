using CoreBackend.Features.Jobs.Domain;
using CoreBackend.Messages;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace CoreBackend.Infrastructure.Rebus;

public class ExistingTransactionJobCreator(NhibernateUnitOfWork unitOfWork) : BaseJobCreator
{
    public override async Task<Job> CreateJob(
        BaseMessage baseMessage,
        Guid jobGuid, 
        bool shouldMarkJobAsHandlingStarted,
        string? handlingQueue
    )
    {
        return await CreateJobWithinUnitOfWork(baseMessage, jobGuid, shouldMarkJobAsHandlingStarted, unitOfWork, handlingQueue);
    }
}