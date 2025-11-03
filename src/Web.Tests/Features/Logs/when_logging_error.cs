using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Logs;

namespace MrWatchdog.Web.Tests.Features.Logs;

[TestFixture]
public class when_logging_error
{
    private ILogger<LogsController> _logger = null!;
    private ICoreBus _bus = null!;
    private LogsController _logsController = null!;
    private IOptions<EmailAddressesOptions> _iEmailAddressesOptions = null!;

    [SetUp]
    public async Task Context()
    {
        _logger = A.Fake<ILogger<LogsController>>();
        _bus = A.Fake<ICoreBus>();

        var iLoggingOptions = OptionsTestRetriever.Retrieve<LoggingOptions>();
        _iEmailAddressesOptions = OptionsTestRetriever.Retrieve<EmailAddressesOptions>();

        _logsController = new LogsController(
            _logger,
            iLoggingOptions,
            _bus,
            _iEmailAddressesOptions,
            OptionsTestRetriever.Retrieve<RuntimeOptions>()
        )
        {
            ControllerContext = new ControllerContext(new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ControllerActionDescriptor())
            )
        };
        _logsController.ControllerContext.HttpContext.Request.Headers[LogConstants.LogErrorApiSecretHeaderName] 
            = iLoggingOptions.Value.LogErrorApiSecret;

        await _logsController.LogError(
            """
            JS error: {"message":"Watchdog web page name is missing.","name":"Error","source":"https://mrwatchdog_test/assets/bundle.p4xqk0duio.js","lineno":15,"colno":44916,"stack":"onWatchdogWebPageNameModified
            """);
    }

    [Test]
    public void send_email_command_is_sent()
    {
        A.CallTo(() => _bus.Send(A<SendEmailCommand>.That.Matches(p => _MatchingCommand(p)))).MustHaveHappenedOnceExactly();
    }

    private bool _MatchingCommand(SendEmailCommand command)
    {
        command.RecipientEmail.ShouldBe(_iEmailAddressesOptions.Value.FrontendErrors);
        command.Subject.ShouldBe(
            """
            Test JS error: {"message":"Watchdog web page name is missing.","name":"Error","source
            """
        );
        command.HtmlMessage.ShouldContain("RequestId:");
        command.HtmlMessage.ShouldContain("TimeStamp:");
        command.HtmlMessage.ShouldContain(
            """
            JS error: {"message":"Watchdog web page name is missing.","name":"Error","source":"https://mrwatchdog_test/assets/bundle.p4xqk0duio.js","lineno":15,"colno":44916,"stack":"onWatchdogWebPageNameModified
            """
        );
        return true;
    }
}