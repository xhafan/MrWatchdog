using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.WatchdogOwnerOrSuperAdminAuthorizationHandlers;

[TestFixture]
public class when_checking_authorization_for_acting_user_being_the_same_as_watchdog_owner : BaseDatabaseTest
{
    private AuthorizationHandlerContext _authorizationHandlerContext = null!;

    [SetUp]
    public async Task Context()
    {
        var user = new UserBuilder(UnitOfWork)
            .WithSuperAdmin(false)
            .Build();

        var watchdog = new WatchdogBuilder(UnitOfWork)
            .WithUser(user)
            .Build();

        var handler = new WatchdogOwnerOrSuperAdminAuthorizationHandler(
            new UserRepository(UnitOfWork),
            new NhibernateRepository<Watchdog>(UnitOfWork)
        );

        _authorizationHandlerContext = new AuthorizationHandlerContext(
            [new WatchdogOwnerOrSuperAdminRequirement()],
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, $"{user.Id}")], authenticationType: "Test")),
            resource: watchdog.Id
        );

        await handler.HandleAsync(
            _authorizationHandlerContext
        );
    }

    [Test]
    public void authorization_succeeds()
    {
        _authorizationHandlerContext.HasSucceeded.ShouldBe(true);
    }
}