using MrWatchdog.Core.Features.Scrapers.Commands;
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
                new ScrapeScraperCommand(23)
            )
        );

        destinationAddress.ShouldBe($"Test{RebusQueues.Scraping}");
    }

    [Test]
    public async Task for_enable_scraper_web_page_command()
    {
        var destinationAddress = await _messageRouter.GetDestinationAddress(
            new Message(
                new Dictionary<string, string>(),
                new EnableScraperWebPageCommand(23, 24)
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
                new EnableScraperWebPageCommand(23, 24)
            )
        );

        destinationAddress.ShouldBe($"Test{RebusQueues.AdminBulk}");
    }
}