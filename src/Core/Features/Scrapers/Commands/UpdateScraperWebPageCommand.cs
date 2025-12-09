using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record UpdateScraperWebPageCommand(ScraperWebPageArgs ScraperWebPageArgs) : Command;