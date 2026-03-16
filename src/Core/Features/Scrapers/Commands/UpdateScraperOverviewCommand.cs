using CoreBackend.Messages;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record UpdateScraperOverviewCommand(ScraperOverviewArgs ScraperOverviewArgs) : Command;