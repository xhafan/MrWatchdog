using System.Security.Claims;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.ClaimsPrincipalExtensions;

[TestFixture]
public class when_checking_user_is_authenticated
{
    [Test]
    public void authenticated_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "23")], authenticationType: "Test"));

        user.IsAuthenticated().ShouldBe(true);
    }
    
    [Test]
    public void unauthenticated_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([], authenticationType: null));

        user.IsAuthenticated().ShouldBe(false);
    }
}