using CoreBackend.Features.Jobs.Domain;
using CoreBackend.Messages;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace CoreBackend.Infrastructure.Rebus;

public class NewTransactionJobCreator(INhibernateConfigurator nhibernateConfigurator) : BaseJobCreator
{
    public override async Task<Job> CreateJob(
        BaseMessage baseMessage,
        Guid jobGuid, 
        bool shouldMarkJobAsHandlingStarted,
        string? handlingQueue
    )
    {
        return await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(nhibernateConfigurator),
            async newUnitOfWork => await CreateJobWithinUnitOfWork(baseMessage, jobGuid, shouldMarkJobAsHandlingStarted, newUnitOfWork, handlingQueue)
        );
    } 
}