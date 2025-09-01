using FakeItEasy;
using Microsoft.AspNetCore.Http;
using MrWatchdog.Web.Infrastructure.ActingUserAccessors;

namespace MrWatchdog.Web.Tests.Infrastructure.ActingUserAccessors.HttpContextActingUserAccessors;

[TestFixture]
public class when_getting_acting_user_id_without_http_context
{
    private long _actingUserId;

    [SetUp]
    public void Context()
    {
        var httpContextAccessor = A.Fake<IHttpContextAccessor>();
        A.CallTo(() => httpContextAccessor.HttpContext).Returns(null);
        var actingUserAccessor = new HttpContextActingUserAccessor(httpContextAccessor);

        _actingUserId = actingUserAccessor.GetActingUserId();
    }

    [Test]
    public void acting_user_id_is_correct()
    {
        _actingUserId.ShouldBe(0);
    }
}