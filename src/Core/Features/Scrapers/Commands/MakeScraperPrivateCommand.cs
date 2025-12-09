using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record MakeScraperPrivateCommand(long ScraperId) : Command;