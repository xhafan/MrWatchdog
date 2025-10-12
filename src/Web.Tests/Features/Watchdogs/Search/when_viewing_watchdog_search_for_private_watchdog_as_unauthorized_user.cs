using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Search;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Search;

[TestFixture]
public class when_viewing_watchdog_search_for_private_watchdog_as_unauthorized_user : BaseDatabaseTest
{
    private SearchModel _model = null!;
    private WatchdogSearch _watchdogSearch = null!;
    private Watchdog _watchdog = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var authorizationService = A.Fake<IAuthorizationService>();
        A.CallTo(() => authorizationService.AuthorizeAsync(
                A<ClaimsPrincipal>._,
                _watchdogSearch.Id,
                A<IAuthorizationRequirement[]>.That.Matches(p => p.OfType<WatchdogSearchOwnerOrSuperAdminRequirement>().Any())
            ))
            .Returns(AuthorizationResult.Failed());
        
        _model = new SearchModelBuilder(UnitOfWork)
            .WithAuthorizationService(authorizationService)
            .Build();
        
        _actionResult = await _model.OnGet(_watchdogSearch.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<ForbidResult>();
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["<div>text 1</div>"]);
        _watchdog.EnableWebPage(watchdogWebPage.Id);
        
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm("text")
            .Build();

        UnitOfWork.Flush();
    }
}