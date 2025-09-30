using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.WatchdogAlertOwnerOrSuperAdminAuthorizationHandlers;

[TestFixture]
public class when_checking_authorization_for_acting_user_as_superadmin : BaseDatabaseTest
{
    private AuthorizationHandlerContext _authorizationHandlerContext = null!;

    [SetUp]
    public async Task Context()
    {
        var superAdminUser = new UserBuilder(UnitOfWork)
            .WithSuperAdmin(true)
            .Build();

        var anotherUser = new UserBuilder(UnitOfWork)
            .WithSuperAdmin(false)
            .Build();

        var watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithUser(anotherUser)
            .Build();

        var handler = new WatchdogAlertOwnerOrSuperAdminAuthorizationHandler(
            new UserRepository(UnitOfWork),
            new NhibernateRepository<WatchdogAlert>(UnitOfWork)
        );

        _authorizationHandlerContext = new AuthorizationHandlerContext(
            [new WatchdogAlertOwnerOrSuperAdminRequirement()],
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, $"{superAdminUser.Id}")], authenticationType: "Test")),
            resource: watchdogAlert.Id
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