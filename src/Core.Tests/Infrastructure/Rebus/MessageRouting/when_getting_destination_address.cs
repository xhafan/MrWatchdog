using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.MessageRouting;
using Rebus.Messages;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.MessageRouting;

[TestFixture]
public class when_getting_destination_address
{
    private MessageRouter _messageRouter = null!;

    [SetUp]
    public void Context()
    {
        _messageRouter = new MessageRouter("Test");
    }

    [Test]
    public async Task for_scraping_command()
    {
        var destinationAddress = await _messageRouter.GetDestinationAddress(
            new Message(
                new Dictionary<string, string>(),
                new ScrapeWatchdogCommand(23)
            )
        );

        destinationAddress.ShouldBe($"Test{RebusQueues.Scraping}");
    }

    [Test]
    public async Task for_enable_watchdog_wen_page_command()
    {
        var destinationAddress = await _messageRouter.GetDestinationAddress(
            new Message(
                new Dictionary<string, string>(),
                new EnableWatchdogWebPageCommand(23, 24)
            )
        );

        destinationAddress.ShouldBe($"Test{RebusQueues.Main}");
    }

    [Test]
    public async Task for_command_with_queue_redirection()
    {
        var destinationAddress = await _messageRouter.GetDestinationAddress(
            new Message(
                new Dictionary<string, string>
                {
                    {CustomHeaders.QueueForRedirection, $"Test{RebusQueues.AdminBulk}"}
                },
                new EnableWatchdogWebPageCommand(23, 24)
            )
        );

        destinationAddress.ShouldBe($"Test{RebusQueues.AdminBulk}");
    }
}