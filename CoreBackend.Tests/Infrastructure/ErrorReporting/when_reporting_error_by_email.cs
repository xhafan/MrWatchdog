using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Messages;
using FakeItEasy;
using Microsoft.Extensions.Options;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture]
public class when_reporting_error_by_email
{
    [Test]
    public async Task it_sends_an_encoded_frontend_error_email()
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
            FrontendErrors = "frontend-errors@smart-guide.org"
        }));

        await reporter.Report(new ErrorReport(message, "request-123", occurredAtUtc));

        sentCommand.ShouldNotBeNull();
        sentCommand.RecipientEmail.ShouldBe("frontend-errors@smart-guide.org");
        sentCommand.Subject.ShouldBe(message[..80]);
        sentCommand.HtmlMessage.ShouldContain("RequestId: request-123");
        sentCommand.HtmlMessage.ShouldContain("TimeStamp: 2026-09-01 10:11:12.345 UTC");
        sentCommand.HtmlMessage.ShouldContain("&lt;script&gt;alert(&#39;failure&#39;)&lt;/script&gt;");
        sentCommand.HtmlMessage.ShouldNotContain("<script>");
        A.CallTo(() => bus.Send(A<Command>._)).MustHaveHappenedOnceExactly();
    }
}
