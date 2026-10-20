using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Configurations;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Rebus.ErrorHandlers;
using CoreBackend.Infrastructure.Jsons;
using CoreBackend.Messages;
using FakeItEasy;
using Microsoft.Extensions.Options;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture]
public class when_reporting_error_by_email
{
    [Test]
    public async Task it_sends_an_encoded_backend_error_email()
    {
        var bus = A.Fake<ICoreBus>();
        SendEmailCommand? sentCommand = null;
        A.CallTo(() => bus.Send(A<Command>._))
            .Invokes((Command command) => sentCommand = (SendEmailCommand) command)
            .Returns(Task.CompletedTask);
        var message = $"<script>alert('failure')</script>{new string('x', 100)}";
        var occurredAtUtc = new DateTime(2026, 9, 1, 10, 11, 12, 345, DateTimeKind.Utc);
        var reporter = new EmailErrorReporter(bus, Options.Create(new EmailAddressesOptions
        {
            BackendErrors = "backend-errors@smart-guide.org",
            FrontendErrors = "frontend-errors@smart-guide.org"
        }), Options.Create(new RuntimeOptions { Url = "https://mrwatchdog_test" }));

        await reporter.Report(new ErrorReport(message, "request-123", occurredAtUtc, Origin: ErrorReportOrigin.Server));

        sentCommand.ShouldNotBeNull();
        sentCommand.RecipientEmail.ShouldBe("backend-errors@smart-guide.org");
        sentCommand.Subject.ShouldBe(message[..80]);
        sentCommand.HtmlMessage.ShouldContain("RequestId: request-123");
        sentCommand.HtmlMessage.ShouldContain("TimeStamp: 2026-09-01 10:11:12.345 UTC");
        sentCommand.HtmlMessage.ShouldContain("&lt;script&gt;alert(&#39;failure&#39;)&lt;/script&gt;");
        sentCommand.HtmlMessage.ShouldNotContain("<script>");
        sentCommand.IsErrorNotification.ShouldBeTrue();
        A.CallTo(() => bus.Send(A<Command>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task it_preserves_the_frontend_recipient_for_browser_errors()
    {
        var bus = A.Fake<ICoreBus>();
        var reporter = _reporter(bus);

        await reporter.Report(new ErrorReport("Browser interaction failed", "request-123", DateTime.UtcNow,
            Origin: ErrorReportOrigin.Browser));

        A.CallTo(() => bus.Send(A<SendEmailCommand>.That.Matches(command =>
                command.RecipientEmail == "frontend-errors@smart-guide.org"
                && command.Subject == "Browser interaction failed" && command.IsErrorNotification)))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task it_preserves_the_failed_job_subject_and_link_with_encoded_error_details()
    {
        var bus = A.Fake<ICoreBus>();
        SendEmailCommand? sent = null;
        A.CallTo(() => bus.Send(A<Command>._)).Invokes((Command command) => sent = (SendEmailCommand) command);
        var jobGuid = Guid.NewGuid();
        var failure = new FailedMessageReport(jobGuid, typeof(TestCommand), "request-123", "<script>failure</script>", DateTime.UtcNow);

        await _reporter(bus).Report(new ErrorReport(failure.ErrorDetails, "request-123", failure.OccurredAtUtc,
            Origin: ErrorReportOrigin.FailedJob, FailedMessage: failure));

        sent.ShouldNotBeNull();
        sent.RecipientEmail.ShouldBe("backend-errors@smart-guide.org");
        sent.Subject.ShouldBe("Job TestCommand failed");
        sent.HtmlMessage.ShouldContain($"<a href=\"https://mrwatchdog_test/api/Jobs/{jobGuid}\">TestCommand</a>");
        sent.HtmlMessage.ShouldContain("RequestId: request-123");
        sent.HtmlMessage.ShouldContain("&lt;script&gt;failure&lt;/script&gt;");
        sent.HtmlMessage.ShouldNotContain("<script>");
        sent.IsErrorNotification.ShouldBeTrue();
    }

    [Test]
    public void it_preserves_the_notification_marker_when_serialized_for_rebus()
    {
        var command = new SendEmailCommand("errors@example.test", "Error", "Details", IsErrorNotification: true);

        JsonHelper.Deserialize<SendEmailCommand>(JsonHelper.Serialize(command))
            .ShouldNotBeNull().IsErrorNotification.ShouldBeTrue();
        JsonHelper.Deserialize<SendEmailCommand>("""{"recipientEmail":"user@example.test","subject":"Normal email","htmlMessage":"Body"}""")
            .ShouldNotBeNull().IsErrorNotification.ShouldBeFalse();
    }

    private static EmailErrorReporter _reporter(ICoreBus bus) => new(bus, Options.Create(new EmailAddressesOptions
    {
        BackendErrors = "backend-errors@smart-guide.org",
        FrontendErrors = "frontend-errors@smart-guide.org"
    }), Options.Create(new RuntimeOptions { Url = "https://mrwatchdog_test" }));

    private sealed record TestCommand : Command;
}
