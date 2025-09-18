using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Security.Claims;
using MrWatchdog.Core.Features.Account;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.ClaimsPrincipalExtensions;

[TestFixture]
public class when_checking_user_is_superadmin
{
    [Test]
    public void non_superadmin_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "23")], authenticationType: "Test"));

        user.IsSuperAdmin().ShouldBe(false);
    }
    
    [Test]
    public void superadmin_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "23"),
            new Claim(CustomClaimTypes.SuperAdmin, "true")
        ], authenticationType: "Test"));

        user.IsSuperAdmin().ShouldBe(true);
    }
    
    [Test]
    public void unauthenticated_superadmin_user()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "23"),
            new Claim(CustomClaimTypes.SuperAdmin, "true")
        ], authenticationType: null));

        user.IsSuperAdmin().ShouldBe(false);
    }
}