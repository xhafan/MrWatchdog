using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreUtils;

namespace MrWatchdog.TestsShared;

public abstract class BaseIntegrationTest
{
    protected NhibernateUnitOfWork UnitOfWork = null!;
    protected static ICollection<IDomainEvent> RaisedDomainEvents
    {
        get
        {
            Guard.Hope(TestFixtureContext.RaisedDomainEvents.Value != null, "TestFixtureContext.RaisedDomainEvents.Value" + " is null");
            return TestFixtureContext.RaisedDomainEvents.Value;
        }
    }

    [SetUp]
    public void BaseSetUp()
    {
        TestFixtureContext.RaisedDomainEvents.Value = new List<IDomainEvent>();
        UnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
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
        
        TestFixtureContext.RaisedDomainEvents.Value = null;
    }
}