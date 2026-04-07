using CoreBackend.Infrastructure.Rebus.ErrorHandlers;
using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Commands;
using Rebus.Messages;
using Rebus.Retry;
using Rebus.Serialization;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.ErrorHandlers.ReportFailedMessageErrorHandlers;

[TestFixture]
public class when_handling_failed_message
{
    private IErrorHandler _originalErrorHandler = null!;
    private ITransactionContext _transactionContext = null!;
    private TransportMessage _transportMessage = null!;
    private ExceptionInfo _exceptionInfo = null!;
    private IFailedMessageReporter _failedMessageReporter = null!;
    private Guid _jobGuid;

    [SetUp]
    public async Task Context()
    {
        _originalErrorHandler = A.Fake<IErrorHandler>();

        var serializer = A.Fake<ISerializer>();
        _failedMessageReporter = A.Fake<IFailedMessageReporter>();

        var errorHandlerDecorator = new ReportFailedMessageErrorHandler(
            _originalErrorHandler,
            serializer,
            _failedMessageReporter
        );

        _jobGuid = Guid.NewGuid();

        _transportMessage = new TransportMessage(new Dictionary<string, string>(), [1, 2, 3]);
        A.CallTo(() => serializer.Deserialize(_transportMessage)).Returns(
            new Message(
                new Dictionary<string, string> {{Headers.MessageId, _jobGuid.ToString()}},
                new ArchiveScraperCommand(ScraperId: 23)
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
    public void failed_message_is_reported()
    {
        A.CallTo(() => _failedMessageReporter.Report(_jobGuid, typeof(ArchiveScraperCommand))).MustHaveHappenedOnceExactly();
    }
}
