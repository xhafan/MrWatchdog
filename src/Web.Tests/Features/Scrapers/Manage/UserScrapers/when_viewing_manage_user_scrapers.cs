using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Manage.UserScrapers;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Manage.UserScrapers;

[TestFixture]
public class when_viewing_manage_user_scrapers : BaseDatabaseTest
{
    private Scraper _scraperForUserOne = null!;
    private UserScrapersModel _model = null!;
    private User _userOne = null!;
    private User _userTwo = null!;
    private Scraper _scraperForUserTwo = null!;
    private Scraper _archivedScraperForUserOne = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new UserScrapersModelBuilder(UnitOfWork)
            .WithActingUser(_userOne)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.UserScrapers.ShouldContain(
            new GetUserScrapersQueryResult
            {
                ScraperId = _scraperForUserOne.Id, 
                ScraperName = "scraper user one",
                PublicStatus = PublicStatus.Public
            }
        );
        _model.UserScrapers.Any(x => x.ScraperId == _scraperForUserTwo.Id).ShouldBe(false);
        _model.UserScrapers.Any(x => x.ScraperId == _archivedScraperForUserOne.Id).ShouldBe(false);
    }    
    
    private void _BuildEntities()
    {
        _userOne = new UserBuilder(UnitOfWork).Build();
        _userTwo = new UserBuilder(UnitOfWork).Build();

        _scraperForUserOne = new ScraperBuilder(UnitOfWork)
            .WithUser(_userOne)
            .WithName("scraper user one")
            .Build();
        _scraperForUserOne.MakePublic();

        _archivedScraperForUserOne = new ScraperBuilder(UnitOfWork)
            .WithUser(_userOne)
            .Build();
        _archivedScraperForUserOne.Archive();

        _scraperForUserTwo = new ScraperBuilder(UnitOfWork)
            .WithUser(_userTwo)
            .WithName("scraper user two")
            .Build();
        
        UnitOfWork.Flush();
    }
}