using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public class ExistingTransactionJobCreator(NhibernateUnitOfWork unitOfWork) : BaseJobCreator
{
    public override async Task<Job> CreateJob(
        BaseMessage baseMessage,
        Guid jobGuid, 
        bool shouldMarkJobAsHandlingStarted
    )
    {
        return await CreateJobWithinUnitOfWork(baseMessage, jobGuid, shouldMarkJobAsHandlingStarted, unitOfWork);
    }
}