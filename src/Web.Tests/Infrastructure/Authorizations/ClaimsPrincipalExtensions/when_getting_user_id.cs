using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.ClaimsPrincipalExtensions;

[TestFixture]
public class when_getting_user_id
{
    [Test]
    public void authenticated_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "23")], authenticationType: "Test"));

        user.GetUserId().ShouldBe(23);
    }
    
    [Test]
    public void unauthenticated_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([], authenticationType: null));

        user.GetUserId().ShouldBe(0);
    }
    
    [Test]
    public void authenticated_user_with_invalid_user_id()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "not-a-number")], authenticationType: "Test"));

        Should.Throw<FormatException>(() => user.GetUserId());
    }    
}