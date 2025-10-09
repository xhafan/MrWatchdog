using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.SuperAdminAuthorizationHandlers;

[TestFixture]
public class when_checking_authorization_for_non_superadmin_user : BaseDatabaseTest
{
    private AuthorizationHandlerContext _authorizationHandlerContext = null!;

    [SetUp]
    public async Task Context()
    {
        var user = new UserBuilder(UnitOfWork)
            .WithSuperAdmin(false)
            .Build();

        var handler = new SuperAdminAuthorizationHandler(new UserRepository(UnitOfWork));

        _authorizationHandlerContext = new AuthorizationHandlerContext(
            [new SuperAdminRequirement()],
            new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, $"{user.Id}")],
                authenticationType: CookieAuthenticationDefaults.AuthenticationScheme)
            ),
            resource: null
        );

        await handler.HandleAsync(
            _authorizationHandlerContext
        );
    }

    [Test]
    public void authorization_fails()
    {
        _authorizationHandlerContext.HasSucceeded.ShouldBe(false);
    }
}