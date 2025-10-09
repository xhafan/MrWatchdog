using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.ClaimsPrincipalExtensions;

[TestFixture]
public class when_checking_user_is_authenticated
{
    [Test]
    public void authenticated_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "23")],
            authenticationType: CookieAuthenticationDefaults.AuthenticationScheme)
        );

        user.IsAuthenticated().ShouldBe(true);
    }
    
    [Test]
    public void unauthenticated_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([], authenticationType: null));

        user.IsAuthenticated().ShouldBe(false);
    }

    [Test]
    public void authenticated_user_by_external_provider_google()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([], authenticationType: "Google"));

        user.IsAuthenticated().ShouldBe(false);
    }
}