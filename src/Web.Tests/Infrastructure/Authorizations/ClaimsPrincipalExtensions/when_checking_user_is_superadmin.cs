using Microsoft.AspNetCore.Authentication.Cookies;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.ClaimsPrincipalExtensions;

[TestFixture]
public class when_checking_user_is_superadmin : BaseDatabaseTest
{
    [Test]
    public async Task non_superadmin_user()
    {
        var user = new UserBuilder(UnitOfWork)
            .WithSuperAdmin(false)
            .Build();

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, $"{user.Id}")],
            authenticationType: CookieAuthenticationDefaults.AuthenticationScheme)
        );

        (await claimsPrincipal.IsSuperAdmin(new UserRepository(UnitOfWork))).ShouldBe(false);
    }
    
    [Test]
    public async Task superadmin_user()
    {
        var superAdminUser = new UserBuilder(UnitOfWork)
            .WithSuperAdmin(true)
            .Build();

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, $"{superAdminUser.Id}")
            ],
            authenticationType: CookieAuthenticationDefaults.AuthenticationScheme)
        );

        (await claimsPrincipal.IsSuperAdmin(new UserRepository(UnitOfWork))).ShouldBe(true);
    }
    
    [Test]
    public async Task unauthenticated_user()
    {
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([], authenticationType: null));

        (await claimsPrincipal.IsSuperAdmin(new UserRepository(UnitOfWork))).ShouldBe(false);
    }
}