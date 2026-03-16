using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Repositories;
using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingAndCompletionIncomingSteps;

public class JobCompletionIncomingStepBuilder(NhibernateUnitOfWork unitOfWork)
{
    public JobCompletionIncomingStep Build()
    {
        var jobRepositoryFactory = A.Fake<IJobRepositoryFactory>();
        A.CallTo(() => jobRepositoryFactory.Create()).Returns(new JobRepository(unitOfWork));

        return new JobCompletionIncomingStep(jobRepositoryFactory);
    }
}

