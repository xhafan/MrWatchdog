using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.UserScrapers;

namespace MrWatchdog.Web.Tests.Features.Scrapers.UserScrapers;

[TestFixture]
public class when_viewing_user_scrapers : BaseDatabaseTest
{
    private Scraper _publicScraper = null!;
    private UserScrapersModel _model = null!;
    private Scraper _privateUserScraper = null!;
    private User _user = null!;
    private Scraper _makePublicRequestedUserScraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new UserScrapersModelBuilder(UnitOfWork)
            .WithActingUser(_user)
            .Build();
        
        await _model.OnGet();
    }
   
    [Test]
    public void user_scrapers_are_fetched()
    {
        _model.UserScrapers.Count().ShouldBe(2);
        _model.UserScrapers.ShouldContain(
            new GetUserScrapersQueryResult
            {
                ScraperId = _privateUserScraper.Id, 
                ScraperName = "private user scraper",
                PublicStatus = PublicStatus.Private
            }
        );  
        _model.UserScrapers.ShouldContain(
            new GetUserScrapersQueryResult
            {
                ScraperId = _makePublicRequestedUserScraper.Id, 
                ScraperName = "make public requested user scraper",
                PublicStatus = PublicStatus.RequestedToBeMadePublic
            }
        );  
    }   
    
    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
    
    private void _BuildEntities()
    {
        _publicScraper = new ScraperBuilder(UnitOfWork)
            .WithName("public scraper")
            .Build();
        _publicScraper.MakePublic();
        
        _user = new UserBuilder(UnitOfWork).Build();
        
        _privateUserScraper = new ScraperBuilder(UnitOfWork)
            .WithName("private user scraper")
            .WithUser(_user)
            .Build();
        
        _makePublicRequestedUserScraper = new ScraperBuilder(UnitOfWork)
            .WithName("make public requested user scraper")
            .WithUser(_user)
            .Build();
        _makePublicRequestedUserScraper.RequestToMakePublic();

        // ReSharper disable once UnusedVariable
        var anotherPrivateScraper = new ScraperBuilder(UnitOfWork)
            .WithName("another user private scraper")
            .Build();
        
        UnitOfWork.Flush();
    }
}