using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.WatchdogSearchOwnerOrSuperAdminAuthorizationHandlers;

[TestFixture]
public class when_checking_authorization_for_acting_user_being_different_than_watchdog_search_owner : BaseDatabaseTest
{
    private AuthorizationHandlerContext _authorizationHandlerContext = null!;

    [SetUp]
    public async Task Context()
    {
        var user = new UserBuilder(UnitOfWork)
            .WithSuperAdmin(false)
            .Build();

        var anotherUser = new UserBuilder(UnitOfWork)
            .WithSuperAdmin(false)
            .Build();

        var watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithUser(anotherUser)
            .Build();

        var handler = new WatchdogSearchOwnerOrSuperAdminAuthorizationHandler(
            new UserRepository(UnitOfWork),
            new NhibernateRepository<WatchdogSearch>(UnitOfWork)
        );

        _authorizationHandlerContext = new AuthorizationHandlerContext(
            [new WatchdogSearchOwnerOrSuperAdminRequirement()],
            new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, $"{user.Id}")], 
                authenticationType: CookieAuthenticationDefaults.AuthenticationScheme)
            ),
            resource: watchdogSearch.Id
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