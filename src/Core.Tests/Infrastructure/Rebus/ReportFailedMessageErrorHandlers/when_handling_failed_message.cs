using FakeItEasy;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using Rebus.Messages;
using Rebus.Retry;
using Rebus.Serialization;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.ReportFailedMessageErrorHandlers;

[TestFixture]
public class when_handling_failed_message
{
    private IErrorHandler _originalErrorHandler = null!;
    private ITransactionContext _transactionContext = null!;
    private TransportMessage _transportMessage = null!;
    private ExceptionInfo _exceptionInfo = null!;
    private IEmailSender _emailSender = null!;
    private IOptions<EmailAddressesOptions> _iEmailAddressesOptions = null!;
    private Guid _jobGuid;

    [SetUp]
    public async Task Context()
    {
        _originalErrorHandler = A.Fake<IErrorHandler>();
        
        var serializer = A.Fake<ISerializer>();
        _emailSender = A.Fake<IEmailSender>();

        _iEmailAddressesOptions = OptionsTestRetriever.Retrieve<EmailAddressesOptions>();

        var errorHandlerDecorator = new ReportFailedMessageErrorHandler(
            _originalErrorHandler,
            serializer,
            _emailSender,
            OptionsTestRetriever.Retrieve<RuntimeOptions>(),
            _iEmailAddressesOptions
        );

        _jobGuid = Guid.NewGuid();

        _transportMessage = new TransportMessage(new Dictionary<string, string>(), [1, 2, 3]);
        A.CallTo(() => serializer.Deserialize(_transportMessage)).Returns(
            new Message(
                new Dictionary<string, string> {{Headers.MessageId, _jobGuid.ToString()}},
                new ArchiveWatchdogCommand(WatchdogId: 23)
            )
        );

        _transactionContext = A.Fake<ITransactionContext>();
        _exceptionInfo = new ExceptionInfo(
            "Exception",
            "Test exception",
            "Exception details",
            DateTimeOffset.UtcNow
        );


        await errorHandlerDecorator.HandlePoisonMessage(
            _transportMessage,
            _transactionContext,
            _exceptionInfo
        );
    }

    [Test]
    public void original_error_handler_is_executed()
    {
        A.CallTo(() =>
                _originalErrorHandler.HandlePoisonMessage(_transportMessage, _transactionContext, _exceptionInfo))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void email_about_failed_message_is_sent()
    {
        A.CallTo(() => _emailSender.SendEmail(
                _iEmailAddressesOptions.Value.BackendErrors,
                A<string>.That.Matches(p => p.Contains("Job") && p.Contains("failed")),
                A<string>.That.Matches(p => p.Contains("Job") && p.Contains("failed")
                                            && p.Contains($"""
                                                           <a href="https://mrwatchdog_test/api/Jobs/{_jobGuid}">ArchiveWatchdogCommand</a>
                                                           """)
                )
            ))
            .MustHaveHappenedOnceExactly();
    }
}