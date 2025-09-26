using FakeItEasy;
using Microsoft.AspNetCore.Http;
using MrWatchdog.Web.Infrastructure.ActingUserAccessors;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Infrastructure.ActingUserAccessors.HttpContextActingUserAccessors;

[TestFixture]
public class when_getting_acting_user_id_for_authenticated_user
{
    private long _actingUserId;

    [SetUp]
    public void Context()
    {
        var httpContextAccessor = A.Fake<IHttpContextAccessor>();
        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "23")], authenticationType: "Test"))
            });
        var actingUserAccessor = new HttpContextActingUserAccessor(httpContextAccessor);

        _actingUserId = actingUserAccessor.GetActingUserId();
    }

    [Test]
    public void acting_user_id_is_correct()
    {
        _actingUserId.ShouldBe(23);
    }
}