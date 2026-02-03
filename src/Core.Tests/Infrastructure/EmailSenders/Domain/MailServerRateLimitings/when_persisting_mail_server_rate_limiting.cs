using MrWatchdog.Core.Infrastructure.EmailSenders.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.Domain.MailServerRateLimitings;

[TestFixture]
public class when_persisting_mail_server_rate_limiting : BaseDatabaseTest
{
    private MailServerRateLimiting _newMailServerRateLimiting = null!;
    private MailServerRateLimiting? _persistedMailServerRateLimiting;
    private DateTime _lastRateLimitedOn;

    [SetUp]
    public void Context()
    {
        _lastRateLimitedOn = DateTime.UtcNow;
        _newMailServerRateLimiting = new MailServerRateLimiting("gmail-smtp-in.l.google.com.", _lastRateLimitedOn);
        UnitOfWork.Save(_newMailServerRateLimiting);
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedMailServerRateLimiting = UnitOfWork.Get<MailServerRateLimiting>(_newMailServerRateLimiting.Id);
    }

    [Test]
    public void persisted_mail_server_rate_limiting_can_be_retrieved_and_has_correct_data()
    {
        _persistedMailServerRateLimiting.ShouldNotBeNull();
        _persistedMailServerRateLimiting.ShouldBe(_newMailServerRateLimiting);

        _persistedMailServerRateLimiting.MailServerName.ShouldBe("gmail-smtp-in.l.google.com.");
        _persistedMailServerRateLimiting.LastRateLimitedOn.ShouldBe(_lastRateLimitedOn, tolerance: TimeSpan.FromMilliseconds(1));
    }
}