using CoreBackend.Infrastructure.Configurations;
using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Rebus.ErrorHandlers;
using CoreBackend.TestsShared;
using FakeItEasy;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Scrapers.Commands;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.ErrorHandlers.FailedMessageEmailReporters;

[TestFixture]
public class when_reporting_failed_message
{
    private ICoreBus _bus = null!;
    private IOptions<EmailAddressesOptions> _iEmailAddressesOptions = null!;
    private Guid _jobGuid;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<ICoreBus>();

        _iEmailAddressesOptions = OptionsTestRetriever.Retrieve<EmailAddressesOptions>();

        var reporter = new FailedMessageEmailReporter(
            _bus,
            OptionsTestRetriever.Retrieve<RuntimeOptions>(),
            _iEmailAddressesOptions
        );

        _jobGuid = Guid.NewGuid();

        await reporter.Report(_jobGuid, typeof(ArchiveScraperCommand));
    }

    [Test]
    public void send_email_command_is_sent()
    {
        A.CallTo(() => _bus.Send(A<SendEmailCommand>.That.Matches(p => _MatchingCommand(p)))).MustHaveHappenedOnceExactly();
    }

    private bool _MatchingCommand(SendEmailCommand command)
    {
        command.RecipientEmail.ShouldBe(_iEmailAddressesOptions.Value.BackendErrors);

        command.Subject.ShouldBe("Job ArchiveScraperCommand failed");

        command.HtmlMessage.ShouldContain("Job");
        command.HtmlMessage.ShouldContain(
            $"""
             <a href="https://mrwatchdog_test/api/Jobs/{_jobGuid}">ArchiveScraperCommand</a>
             """
        );
        command.HtmlMessage.ShouldContain("failed");
        return true;
    }
}
