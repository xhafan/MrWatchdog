using CoreDdd.Domain.Events;
using CoreUtils;

namespace MrWatchdog.TestsShared;

public abstract class BaseTest
{
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
    }

    [TearDown]
    public void BaseTearDown()
    {
        TestFixtureContext.RaisedDomainEvents.Value = null;
    }
}