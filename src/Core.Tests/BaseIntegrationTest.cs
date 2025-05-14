using CoreDdd.Nhibernate.UnitOfWorks;

namespace MrWatchdog.Core.Tests;

public abstract class BaseIntegrationTest
{
    protected NhibernateUnitOfWork UnitOfWork = null!;

    [SetUp]
    public void BaseSetUp()
    {
        UnitOfWork = new NhibernateUnitOfWork(RunOncePerTestRun.NhibernateConfigurator);
        UnitOfWork.BeginTransaction();
    }

    [TearDown]
    public void BaseTearDown()
    {
        try
        {
            UnitOfWork.Flush();
        }
        finally
        {
            UnitOfWork.Rollback();
        }
    }
}