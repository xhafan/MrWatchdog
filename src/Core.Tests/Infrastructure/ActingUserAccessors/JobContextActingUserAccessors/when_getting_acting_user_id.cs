using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Rebus;

namespace MrWatchdog.Core.Tests.Infrastructure.ActingUserAccessors.JobContextActingUserAccessors;

[TestFixture]
public class when_getting_acting_user_id
{
    private long _actingUserId;

    [SetUp]
    public void Context()
    {
        JobContext.ActingUserId.Value = 23;
        var actingUserAccessor = new JobContextActingUserAccessor();

        _actingUserId = actingUserAccessor.GetActingUserId();
    }

    [Test]
    public void acting_user_id_is_correct()
    {
        _actingUserId.ShouldBe(23);
    }
}