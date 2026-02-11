using FakeItEasy;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperRequestedToBeMadePublic;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperRequestedToBeMadePublic;

[TestFixture]
public class when_notifying_admin_about_scraper_requested_to_be_made_public : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private ICoreBus _bus = null!;
    private IOptions<EmailAddressesOptions> _emailAddressesOptions = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _bus = A.Fake<ICoreBus>();
        
        _emailAddressesOptions = OptionsTestRetriever.Retrieve<EmailAddressesOptions>();

        var handler = new NotifyAdminAboutScraperRequestedToBeMadePublicDomainEventMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork),
            _bus,
            OptionsTestRetriever.Retrieve<RuntimeOptions>(),
            OptionsTestRetriever.Retrieve<EmailAddressesOptions>()
        );

        await handler.Handle(new ScraperRequestedToBeMadePublicDomainEvent(_scraper.Id));
    }

    [Test]
    public void email_notification_about_new_scraped_results_is_sent_to_user()
    {
        A.CallTo(() => _bus.Send(A<SendEmailCommand>.That.Matches(p => _MatchingCommand(p)))).MustHaveHappenedOnceExactly();
    }

    private bool _MatchingCommand(SendEmailCommand command)
    {
        command.RecipientEmail.ShouldBe(_emailAddressesOptions.Value.Admin);
        command.Subject.ShouldContain("Epic Games store free game");
        command.Subject.ShouldNotContain(
            """
            { "en": "Epic Games store free game" }
            """
        );
        command.Subject.ShouldContain("requested to be made public");
        command.HtmlMessage.ShouldContain("has been requested to be made public");
        command.HtmlMessage.ShouldContain("Epic Games store free game");
        command.HtmlMessage.ShouldNotContain(
            """
            { "en": "Epic Games store free game" }
            """
        );
        return true;
    }
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName(
                """
                { "en": "Epic Games store free game" }
                """)
            .Build();
    }
}