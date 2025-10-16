﻿using FakeItEasy;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogRequestedToBeMadePublic;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogRequestedToBeMadePublic;

[TestFixture]
public class when_notifying_admins_about_watchdog_requested_to_be_made_public : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private IEmailSender _emailSender = null!;
    private IOptions<EmailAddressesOptions> _emailAddressesOptions = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _emailSender = A.Fake<IEmailSender>();
        
        _emailAddressesOptions = OptionsTestRetriever.Retrieve<EmailAddressesOptions>();

        var handler = new NotifyAdminsAboutWatchdogRequestedToBeMadePublicDomainEventMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            _emailSender,
            OptionsTestRetriever.Retrieve<RuntimeOptions>(),
            OptionsTestRetriever.Retrieve<EmailAddressesOptions>()
        );

        await handler.Handle(new WatchdogRequestedToBeMadePublicDomainEvent(_watchdog.Id));
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
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("Epic Games store free game")
            .Build();
    }
}