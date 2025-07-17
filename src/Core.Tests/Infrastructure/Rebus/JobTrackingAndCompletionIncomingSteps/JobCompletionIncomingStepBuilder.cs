using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;

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

