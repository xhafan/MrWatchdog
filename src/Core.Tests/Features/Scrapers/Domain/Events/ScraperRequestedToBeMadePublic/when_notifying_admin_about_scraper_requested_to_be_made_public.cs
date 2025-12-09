using FakeItEasy;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperRequestedToBeMadePublic;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperRequestedToBeMadePublic;

[TestFixture]
public class when_notifying_admin_about_scraper_requested_to_be_made_public : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private IEmailSender _emailSender = null!;
    private IOptions<EmailAddressesOptions> _emailAddressesOptions = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _emailSender = A.Fake<IEmailSender>();
        
        _emailAddressesOptions = OptionsTestRetriever.Retrieve<EmailAddressesOptions>();

        var handler = new NotifyAdminAboutScraperRequestedToBeMadePublicDomainEventMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork),
            _emailSender,
            OptionsTestRetriever.Retrieve<RuntimeOptions>(),
            OptionsTestRetriever.Retrieve<EmailAddressesOptions>()
        );

        await handler.Handle(new ScraperRequestedToBeMadePublicDomainEvent(_scraper.Id));
    }

    [Test]
    public void email_notification_about_new_scraping_results_is_sent_to_user()
    {
        A.CallTo(() => _emailSender.SendEmail(
                _emailAddressesOptions.Value.Admin,
                A<string>.That.Matches(p => p.Contains("Epic Games store free game") && p.Contains("requested to be made public")),
                A<string>.That.Matches(p => p.Contains("has been requested to be made public")
                                            && p.Contains("Epic Games store free game")
                )
            ))
            .MustHaveHappenedOnceExactly();
    }
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName("Epic Games store free game")
            .Build();
    }
}