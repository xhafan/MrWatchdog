using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Manage.OtherUsersScrapers;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Manage.OtherUsersScrapers;

[TestFixture]
public class when_viewing_manage_other_users_scrapers : BaseDatabaseTest
{
    private Scraper _scraperForUserOne = null!;
    private OtherUsersScrapersModel _model = null!;
    private User _userOne = null!;
    private User _userTwo = null!;
    private Scraper _scraperForUserTwo = null!;
    private Scraper _archivedScraperForUserTwo = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new OtherUsersScrapersModelBuilder(UnitOfWork)
            .WithActingUser(_userOne)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.OtherUsersScrapers.ShouldContain(
            new GetOtherUsersScrapersQueryResult
            {
                ScraperId = _scraperForUserTwo.Id, 
                ScraperName = "scraper user two",
                PublicStatus = PublicStatus.Public,
                UserId = _userTwo.Id,
                UserEmail = _userTwo.Email
            }
        );
        _model.OtherUsersScrapers.Any(x => x.ScraperId == _archivedScraperForUserTwo.Id).ShouldBe(false);
        _model.OtherUsersScrapers.Any(x => x.ScraperId == _scraperForUserOne.Id).ShouldBe(false);
    }    
    
    private void _BuildEntities()
    {
        _userOne = new UserBuilder(UnitOfWork).Build();
        _userTwo = new UserBuilder(UnitOfWork).Build();

        _scraperForUserOne = new ScraperBuilder(UnitOfWork)
            .WithUser(_userOne)
            .WithName("scraper user one")
            .Build();

        _scraperForUserTwo = new ScraperBuilder(UnitOfWork)
            .WithUser(_userTwo)
            .WithName("scraper user two")
            .Build();
        _scraperForUserTwo.MakePublic();
        
        _archivedScraperForUserTwo = new ScraperBuilder(UnitOfWork)
            .WithUser(_userTwo)
            .Build();
        _archivedScraperForUserTwo.Archive();

        UnitOfWork.Flush();
    }
}