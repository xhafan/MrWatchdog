using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public class NewTransactionJobCreator(INhibernateConfigurator nhibernateConfigurator) : BaseJobCreator
{
    public override async Task<Job> CreateJob(
        BaseMessage baseMessage,
        Guid jobGuid, 
        bool shouldMarkJobAsHandlingStarted,
        string? handlingQueue
    )
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(nhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        return await CreateJobWithinUnitOfWork(baseMessage, jobGuid, shouldMarkJobAsHandlingStarted, newUnitOfWork, handlingQueue);
    } 
}