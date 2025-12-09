using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Infrastructure.Authorizations.ScraperOwnerOrSuperAdminAuthorizationHandlers;

[TestFixture]
public class when_checking_authorization_for_acting_user_being_different_than_scraper_owner : BaseDatabaseTest
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

        var scraper = new ScraperBuilder(UnitOfWork)
            .WithUser(anotherUser)
            .Build();

        var handler = new ScraperOwnerOrSuperAdminAuthorizationHandler(
            new UserRepository(UnitOfWork),
            new NhibernateRepository<Scraper>(UnitOfWork)
        );

        _authorizationHandlerContext = new AuthorizationHandlerContext(
            [new ScraperOwnerOrSuperAdminRequirement()],
            new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, $"{user.Id}")], 
                authenticationType: CookieAuthenticationDefaults.AuthenticationScheme)
            ),
            resource: scraper.Id
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